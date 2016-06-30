using EventCentric.EventSourcing;
using EventCentric.Publishing.Dto;
using EventCentric.Transport;
using EventCentric.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace EventCentric.Publishing
{
    /// <summary>
    /// This is the endpoints where the clients can connect to in orther to push events (sync).
    /// This is hosted by the server.
    /// </summary>
    public class OcassionallyConnectedSource : IPollableEventSource, IOcassionallyConnectedSourceConsumer
    {
        private readonly ConcurrentBag<PollResponse> producerResponse = new ConcurrentBag<PollResponse>();
        private long consumerEventCollectionVersion = 0;

        private readonly TimeSpan pusherTimeout;

        public OcassionallyConnectedSource(string sourceName, string consumerName)
        {
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(sourceName, nameof(sourceName));
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(consumerName, nameof(consumerName));

            this.SourceName = sourceName;
            this.ConsumerName = consumerName;
            this.pusherTimeout = TimeSpan.FromMinutes(1);
        }

        public string SourceName { get; }

        public string ConsumerName { get; }

        public PollResponse PollEvents(Guid streamId, long eventBufferVersion, string consumerName)
        {
            // We return this, because the filtering is made in the producer side, and this is in the consumer 
            // side
            return this.PollEvents(eventBufferVersion, consumerName);
        }

        public PollResponse PollEvents(long eventBufferVersion, string consumerName)
        {
            this.consumerEventCollectionVersion = eventBufferVersion < 0 ? 0 : eventBufferVersion;

            PollResponse clientResponse;
            while (!this.producerResponse.TryTake(out clientResponse))
                Thread.Sleep(1);

            if (!clientResponse.ErrorDetected
                && clientResponse.NewEventsWereFound
                && clientResponse.ConsumerVersion == this.consumerEventCollectionVersion // only updates if versions are equall. Nasty errors ocurred while trying to synchronize.
                && clientResponse.ProducerVersion > eventBufferVersion
                && clientResponse.NewRawEvents.Max(x => x.EventCollectionVersion) > eventBufferVersion)
            {
                clientResponse = PollResponse.CreateSerializedResponse(false, true, clientResponse.StreamType,
                                    clientResponse.NewRawEvents.Where(x => x.EventCollectionVersion > eventBufferVersion).ToList(), eventBufferVersion, clientResponse.ProducerVersion);
            }
            else
            {
                clientResponse = PollResponse.CreateSerializedResponse(false, false, clientResponse.StreamType, new List<SerializedEvent>(), 0, 0);
            }

            return clientResponse;
        }

        public ServerStatus UpdateConsumer(PollResponse producerResponse)
        {
            if (this.consumerEventCollectionVersion == producerResponse.ConsumerVersion) // only updates if versions are equal. Nasty errors.
            {
                var ecvBefore = this.consumerEventCollectionVersion;

                this.producerResponse.Add(producerResponse);

                var stopwatch = Stopwatch.StartNew();
                while (this.consumerEventCollectionVersion == ecvBefore && this.pusherTimeout > stopwatch.Elapsed)
                    Thread.Sleep(1);
            }

            return new ServerStatus(this.consumerEventCollectionVersion);
        }
    }
}
