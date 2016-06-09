using EventCentric.EventSourcing;
using System;

namespace EventCentric.Handling
{
    public class MessageHandling : IMessageHandling
    {
        public MessageHandling(Guid streamId, Func<IEventSourced> handle, bool deduplicateBeforeHandling)
        {
            this.Ignore = false;
            this.StreamId = streamId;
            this.Handle = handle;
            this.DeduplicateBeforeHandling = deduplicateBeforeHandling;
        }

        private MessageHandling()
        {
            this.Ignore = true;
        }

        public bool Ignore { get; }
        public Guid StreamId { get; }
        public Func<IEventSourced> Handle { get; }
        public bool DeduplicateBeforeHandling { get; set; }

        public static MessageHandling IgnoreHandling => new MessageHandling();
    }

    public static class MessageHandlingExtensions
    {
        public static IMessageHandling WithDeduplication(this IMessageHandling handling)
        {
            ((MessageHandling)handling).DeduplicateBeforeHandling = true;
            return handling;
        }

        public static IMessageHandling WithoutDeduplication(this IMessageHandling handling)
        {
            ((MessageHandling)handling).DeduplicateBeforeHandling = false;
            return handling;
        }
    }
}
