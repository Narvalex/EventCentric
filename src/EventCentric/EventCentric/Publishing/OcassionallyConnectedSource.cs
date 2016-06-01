using EventCentric.EventSourcing;
using EventCentric.Publishing.Dto;
using EventCentric.Transport;
using EventCentric.Utils;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private readonly ConcurrentBag<PollResponse> clientResponse = new ConcurrentBag<PollResponse>();
        private readonly ConcurrentBag<ServerStatus> serverStatus = new ConcurrentBag<ServerStatus>();
        private readonly object lockObject = new object();

        public OcassionallyConnectedSource(string sourceName, string consumerName)
        {
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(sourceName, nameof(sourceName));
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(consumerName, nameof(consumerName));

            this.SourceName = sourceName;
            this.ConsumerName = consumerName;
        }

        public string SourceName { get; }

        public string ConsumerName { get; }

        public PollResponse PollEvents(long eventBufferVersion, string consumerName)
        {
            this.serverStatus.Add(new ServerStatus(eventBufferVersion));

            PollResponse clientResponse;
            while (!this.clientResponse.TryTake(out clientResponse))
                Thread.Sleep(1);

            lock (this.lockObject)
            {
                if (clientResponse.ProducerVersion > eventBufferVersion &&
                    clientResponse.NewRawEvents.Max(x => x.EventCollectionVersion) > eventBufferVersion)
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
        }

        public ServerStatus UpdateConsumer(PollResponse response)
        {
            this.clientResponse.Add(response);

            ServerStatus status;
            while (!this.serverStatus.TryTake(out status))
                Thread.Sleep(1);

            return status;
        }
    }
}
