﻿using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Polling;
using EventCentric.Transport;
using EventCentric.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace EventCentric.Publishing
{
    public abstract class PublisherBase : MicroserviceWorker
    {
        protected readonly IEventDao dao;

        protected readonly string streamType;
        protected long eventCollectionVersion = 0;
        protected readonly int eventsToPushMaxCount;
        protected readonly TimeSpan longPollingTimeout;

        public PublisherBase(string streamType, IBus bus, ILogger log, IEventDao dao, TimeSpan pollTimeout, int eventsToPushMaxCount)
            : base(bus, log)
        {
            Ensure.NotNull(dao, nameof(dao));
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(streamType, nameof(streamType));
            Ensure.Positive(eventsToPushMaxCount, nameof(eventsToPushMaxCount));

            this.streamType = streamType;
            this.dao = dao;
            this.longPollingTimeout = pollTimeout;
            this.eventsToPushMaxCount = eventsToPushMaxCount;
        }

        /// <remarks>
        /// Timeout implementation inspired by: http://stackoverflow.com/questions/5018921/implement-c-sharp-timeout
        /// </remarks>
        public PollResponse PollEvents(long consumerVersion, string consumerName)
        {
            var ecv = this.eventCollectionVersion;

            ICollection<NewRawEvent> newSequentialEvents = new List<NewRawEvent>(this.eventsToPushMaxCount);

            // last received version could be somehow less than 0. I found once that was -1, 
            // and was always pushing "0 events", as the signal r tracing showed (27/10/2015) 
            if (consumerVersion < 0)
                consumerVersion = 0;

            // the consumer says that is more updated than the source. That is an error. Maybe the publisher did not started yet!
            if (ecv < consumerVersion)
                return PollResponse.CreateSerializedResponse(true, false, this.streamType, newSequentialEvents, consumerVersion, ecv);

            bool newEventsWereFound = false;
            var stopwatch = Stopwatch.StartNew();
            while (!this.stopping && stopwatch.Elapsed < this.longPollingTimeout)
            {
                if (ecv == consumerVersion)
                {
                    // consumer is up to date, and now is waiting until something happens!
                    Thread.Sleep(1);
                    ecv = this.eventCollectionVersion; // I Forgot to update!
                }
                // weird error, but is crash proof. Once i had an error where in an infinite loop there was an error saying: Pushing 0 events to....
                // A Charly le paso. Sucede que limpio la base de datos y justo queria entregar un evento y no devolvia nada.
                else if (ecv > consumerVersion)
                {
                    var newEvents = this.dao.FindEvents(consumerVersion, this.eventsToPushMaxCount);

                    // Resilient logic
                    var expectedVersion = consumerVersion + 1;
                    if (newEvents.Length > 0 && newEvents[0].EventCollectionVersion == expectedVersion)
                    {
                        newEventsWereFound = true;

                        newSequentialEvents.Add(newEvents[0]);
                        for (int i = 1; i < newEvents.Length; i++)
                        {
                            expectedVersion += 1;
                            if (newEvents[i].EventCollectionVersion == expectedVersion)
                                newSequentialEvents.Add(newEvents[i]);
                            else
                                break;
                        }

                        break;
                    }
                    else
                    {
                        // Lo que le paso a charly.
                        newEventsWereFound = false;
                        //this.log.Error($"There is an error in the event store or a racy condition. The consumer [{consumerName}] version is {consumerVersion} and the local event collection version should be {this.eventCollectionVersion} but it is not.");
                        //this.bus.Publish(new FatalErrorOcurred(new FatalErrorException($"There is an error in the event store or a racy condition. The consumer [{consumerName}] version is {consumerVersion} and the local event collection version should be {this.eventCollectionVersion} but it is not.")));
                        break;
                    }
                }
                else
                    // bizzare, but helpful to avoid infinite loops
                    break;
            }

            return PollResponse.CreateSerializedResponse(false, newEventsWereFound, this.streamType, newSequentialEvents, consumerVersion, ecv);
        }
    }
}
