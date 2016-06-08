using EventCentric.EventSourcing;
using System;

namespace EventCentric.Handling
{
    public class MessageHandling : IMessageHandling
    {
        public MessageHandling(Guid streamId, Func<IEventSourced> handle, bool deduplicateBeforeHandling)
        {
            this.StreamId = streamId;
            this.Handle = handle;
            this.DeduplicateBeforeHandling = deduplicateBeforeHandling;
        }


        /// <summary>
        /// Constructor for ignoring message
        /// </summary>
        public MessageHandling()
        {
            this.StreamId = Guid.NewGuid(); // this helps concurrent ignores...
            this.Handle = () => null;
            this.DeduplicateBeforeHandling = false;
        }

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
