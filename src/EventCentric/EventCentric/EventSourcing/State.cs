using System;
using System.Collections.Generic;
using System.Linq;

namespace EventCentric.EventSourcing
{
    public class StateOf<T> : EventSourced<T>,
        IUpdatesWhen<AnInvalidOperationExceptionOccurred>,
        IUpdatesWhen<Event>,
        IUpdatesAfterSending<Command>
        where T : class, IEventSourced
    {
        public StateOf(Guid id) : base(id) { }

        public StateOf(Guid id, ISnapshot snapshot) : base(id, snapshot) { }

        public StateOf(Guid id, IEnumerable<IEvent> streamOfEvents) : base(id, streamOfEvents) { }

        public T Update(Event @event) => base.UpdateFromMessage(@event);

        public T Update(params Event[] events) => this.Update(events.AsEnumerable());

        public T Update(IEnumerable<Event> events)
        {
            events.ForEach(e => base.UpdateFromMessage(e));
            return this as T;
        }

        public T UpdateAfterSending(Command command) => base.UpdateFromMessage(command);

        public T UpdateAfterSending(params Command[] commands) => this.UpdateAfterSending(commands.AsEnumerable());

        public T UpdateAfterSending(IEnumerable<Command> commands)
        {
            commands.ForEach(c => base.UpdateFromMessage(c));
            return this as T;
        }

        public T UpdateIf(bool condition, params Event[] events)
        {
            if (condition)
                events.ForEach(e => this.Update(e));
            return this as T;
        }

        public T UpdateAfterSendingIf(bool condition, params Command[] commands)
        {
            if (condition)
                commands.ForEach(c => this.UpdateAfterSending(c));
            return this as T;
        }

        public T Throw(string message) => this.Update(new AnInvalidOperationExceptionOccurred(message));

        public virtual void When(AnInvalidOperationExceptionOccurred e) { }

        public void When(Event e) { }

        public void AfterSending(Command c) { }
    }
}
