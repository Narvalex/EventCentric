using System;
using System.Collections.Generic;

namespace EventCentric.EventSourcing
{
    public abstract class StateOf<T> : EventSourced<T> where T : class, IEventSourced
    {
        public StateOf(Guid id) : base(id) { }

        public StateOf(Guid id, ISnapshot snapshot) : base(id, snapshot) { }

        public StateOf(Guid id, IEnumerable<IEvent> streamOfEvents) : base(id, streamOfEvents) { }

        public T Update(Event @event) => base.UpdateFromMessage(@event);

        public T UpdateAfterSending(Command command) => base.UpdateFromMessage(command);

        public T Throw(string message) => this.Update(new AnInvalidOperationExceptionOccurred(message));
    }
}
