using EventCentric.Log;
using EventCentric.Publishing;
using EventCentric.Publishing.Dto;
using EventCentric.Transport;
using EventCentric.Utils;
using System;
using System.Collections.Concurrent;

namespace EventCentric.Messaging
{
    public class InMemoryEventPublisher : IInMemoryEventPublisher
    {
        private readonly ILogger log;
        private readonly ConcurrentDictionary<string, Func<long, string, PollResponse>> sourcesByStreamType = new ConcurrentDictionary<string, Func<long, string, PollResponse>>();
        private readonly ConcurrentDictionary<string, Func<PollResponse, ServerStatus>> serversByStreamType = new ConcurrentDictionary<string, Func<PollResponse, ServerStatus>>();

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
                serversByStreamType.TryAdd(consumer.SourceName, (response) => consumer.UpdateServer(response));
            }
        }

        public PollResponse PollEvents(string streamType, long fromVersion, string consumerName)
        {
            return this.sourcesByStreamType[streamType].Invoke(fromVersion, consumerName);
        }

        public bool TryUpdateServer(string serverName, PollResponse response, out ServerStatus status)
        {
            status = null;
            Func<PollResponse, ServerStatus> update = null;
            if (this.serversByStreamType.TryGetValue(serverName, out update))
            {
                status = update.Invoke(response);
                return true;
            }

            return false;
        }
    }
}
