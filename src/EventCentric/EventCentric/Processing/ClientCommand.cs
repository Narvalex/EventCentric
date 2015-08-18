using System;

namespace EventCentric.Processing
{
    public class ClientCommand : ICommand
    {
        public ClientCommand(Guid eventId, string streamType)
        {
            this.EventId = eventId;
            this.StreamId = Guid.Empty;
            this.StreamType = streamType;
            this.Version = 0;
        }

        public Guid EventId { get; private set; }

        public Guid StreamId { get; private set; }

        public string StreamType { get; private set; }

        public int Version { get; private set; }
    }
}
