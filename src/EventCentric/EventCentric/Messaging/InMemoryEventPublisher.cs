using EventCentric.Log;
using EventCentric.Publishing;
using EventCentric.Publishing.Dto;
using EventCentric.Transport;
using EventCentric.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EventCentric.Messaging
{
    public class InMemoryEventPublisher : IInMemoryEventPublisher
    {
        private readonly ILogger log;
        private readonly ConcurrentDictionary<string, Func<long, string, PollResponse>> sourcesByStreamType = new ConcurrentDictionary<string, Func<long, string, PollResponse>>();
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, Func<PollResponse, ServerStatus>>> ocassionallyConnectedSourcesByConsumer = new ConcurrentDictionary<string, ConcurrentDictionary<string, Func<PollResponse, ServerStatus>>>();

        public InMemoryEventPublisher(ILogger log)
        {
            Ensure.NotNull(log, nameof(log));

            this.log = log;
        }

        public void Register(IPollableEventSource publisher)
        {
            sourcesByStreamType.TryAdd(publisher.SourceName, (version, name) => publisher.PollEvents(version, name));
            if (publisher is IOcassionallyConnectedSourceConsumer)
            {
                var consumer = publisher as IOcassionallyConnectedSourceConsumer;

                this.ocassionallyConnectedSourcesByConsumer.TryAdd(consumer.ConsumerName, new ConcurrentDictionary<string, Func<PollResponse, ServerStatus>>());
                this.ocassionallyConnectedSourcesByConsumer[consumer.ConsumerName].TryAdd(consumer.SourceName, (response) => consumer.UpdateConsumer(response));
            }
        }

        public PollResponse PollEvents(string streamType, long fromVersion, string consumerName)
        {
            return this.sourcesByStreamType[streamType].Invoke(fromVersion, consumerName);
        }

        public bool TryUpdateConsumer(string consumerName, PollResponse response, out ServerStatus status)
        {
            status = null;
            if (!this.ocassionallyConnectedSourcesByConsumer.ContainsKey(consumerName))
                throw new KeyNotFoundException($"The consumer of name {nameof(consumerName)} is not registered in the system");

            var sources = this.ocassionallyConnectedSourcesByConsumer[consumerName];
            if (!sources.ContainsKey(response.StreamType))
                return false;

            status = sources[response.StreamType].Invoke(response);
            return true;
        }
    }
}
