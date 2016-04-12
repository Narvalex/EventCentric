using EventCentric.Polling;
using EventCentric.Publishing.Dto;
using EventCentric.Transport;
using EventCentric.Utils;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EventCentric.Publishing
{
    public class OcassionallyConnectedSource : IPollableEventSource
    {
        private readonly string requiredVersion;
        private readonly ConcurrentBag<PollResponse> clientResponse = new ConcurrentBag<PollResponse>();
        private readonly ConcurrentBag<ServerStatus> serverStatus = new ConcurrentBag<ServerStatus>();

        public OcassionallyConnectedSource(string sourceName, string requiredVersion)
        {
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(sourceName, nameof(sourceName));
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(requiredVersion, nameof(requiredVersion));

            this.SourceName = sourceName;
            this.requiredVersion = requiredVersion;
        }

        public string SourceName { get; }

        public PollResponse PollEvents(long eventBufferVersion, string consumerName)
        {
            this.serverStatus.Add(new ServerStatus(eventBufferVersion));

            PollResponse clientResponse;
            while (!this.clientResponse.TryTake(out clientResponse))
                Thread.Sleep(1);

            lock (this)
            {
                if (clientResponse.ProducerVersion > eventBufferVersion &&
                    clientResponse.NewRawEvents.Max(x => x.EventCollectionVersion) > eventBufferVersion)
                {
                    clientResponse = new PollResponse(false, true, clientResponse.StreamType,
                                        clientResponse.NewRawEvents.Where(x => x.EventCollectionVersion > eventBufferVersion).ToList(), eventBufferVersion, clientResponse.ProducerVersion);
                }
                else
                {
                    clientResponse = new PollResponse(false, false, clientResponse.StreamType, new List<NewRawEvent>(), 0, 0);
                }

                return clientResponse;
            }
        }

        public ServerStatus UpdateServer(ClientData clientData)
        {
            if (clientData.ClientVersion != this.requiredVersion)
                return new ServerStatus(long.MaxValue);


            this.clientResponse.Add(clientData.PollResponse);

            ServerStatus status;
            while (!this.serverStatus.TryTake(out status))
                Thread.Sleep(1);

            return status;
        }
    }
}
