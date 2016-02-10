﻿using EventCentric.EventSourcing;
using System;

namespace EventCentric.Handling
{
    public class MessageHandling : IMessageHandling
    {
        public MessageHandling(bool shouldBeIgnored, Guid streamId, Func<IEventSourced> handle)
        {
            this.ShouldBeIgnored = shouldBeIgnored;
            this.StreamId = streamId;
            this.Handle = handle;
        }

        public bool ShouldBeIgnored { get; }
        public Guid StreamId { get; }
        public Func<IEventSourced> Handle { get; }
    }
}