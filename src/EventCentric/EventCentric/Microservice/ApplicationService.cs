using EventCentric.EventSourcing;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Microservice;
using EventCentric.Publishing;
using EventCentric.Serialization;
using EventCentric.Transport;
using EventCentric.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace EventCentric
{
    /// <summary>
    /// Domain driven design concept of application service.
    /// </summary>
    public abstract class ApplicationService : INamedEventSource, IPollableEventSource
    {
        protected readonly IGuidProvider guid;
        protected readonly ILogger log;
        protected readonly string streamType;
        private readonly int eventsToPushMaxCount;
        private readonly JsonTextSerializer serializer = new JsonTextSerializer();

        private readonly ConcurrentDictionary<Guid, object> streamLocksById = new ConcurrentDictionary<Guid, object>();
        private readonly ConcurrentQueue<IEvent> messageQueue = new ConcurrentQueue<IEvent>();

        public ApplicationService(IGuidProvider guid, ILogger log, string streamType, int eventsToPushMaxCount)
        {
            Ensure.NotNull(guid, nameof(guid));
            Ensure.NotNull(log, nameof(log));
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(streamType, nameof(streamType));
            Ensure.Positive(eventsToPushMaxCount, "eventsToFlushMaxCount");

            this.guid = guid;
            this.log = log;
            this.streamType = streamType;
            this.eventsToPushMaxCount = eventsToPushMaxCount;
        }

        protected Guid NewGuid() => this.guid.NewGuid();

        public string SourceName => this.streamType;

        protected Guid Send(Guid streamId, Message message)
        {
            var transactionId = this.guid.NewGuid();
            lock (this.streamLocksById.GetOrAdd(streamId, new object()))
            {
                var now = DateTime.UtcNow;
                this.messageQueue.Enqueue(
                    message
                        .AsInProcessMessage(transactionId, streamId)
                        .AsQueuedEvent(streamType, Guid.NewGuid(), InMemoryVersioning.GetNextVersion(), now, now.ToLocalTime()));
            }

            return transactionId;
        }

        public PollResponse PollEvents(long eventBufferVersion, string consumerName)
        {
            var events = new List<IEvent>();

            while (true)
            {
                for (int i = 0; i < this.eventsToPushMaxCount; i++)
                {
                    IEvent e;
                    if (!this.messageQueue.TryDequeue(out e))
                        break;

                    events.Add(e);
                }

                if (events.Count > 0)
                    break;

                Thread.Sleep(1);
            }

            return new PollResponse(false, true, this.streamType, events, events.Count, events.Count + this.messageQueue.Count);
        }
    }
}
