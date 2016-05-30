using EventCentric.EventSourcing;
using System;

namespace EventCentric.Handling
{
    public class MessageHandling : IMessageHandling
    {
        public MessageHandling(bool shouldBeIgnored, Guid streamId, Func<IEventSourced> handle, bool deduplicateBeforeHandling)
        {
            this.ShouldBeIgnored = shouldBeIgnored;
            this.StreamId = streamId;
            this.Handle = handle;
            this.DeduplicateBeforeHandling = deduplicateBeforeHandling;
        }

        public bool ShouldBeIgnored { get; }
        public Guid StreamId { get; }
        public Func<IEventSourced> Handle { get; }
        public bool DeduplicateBeforeHandling { get; set; }
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
