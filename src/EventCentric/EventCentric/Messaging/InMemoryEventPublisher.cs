using EventCentric.Log;
using EventCentric.Publishing;
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

        public InMemoryEventPublisher(ILogger log)
        {
            Ensure.NotNull(log, nameof(log));

            this.log = log;
        }

        public void Register(IPollableEventSource publisher)
        {
            sourcesByStreamType.TryAdd(publisher.SourceName, (version, name) => publisher.PollEvents(version, name));
        }

        public PollResponse PollEvents(string streamType, long fromVersion, string consumerName)
        {
            return this.sourcesByStreamType[streamType].Invoke(fromVersion, consumerName);
        }
    }
}
