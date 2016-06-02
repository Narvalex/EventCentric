using System;
using System.Collections.Generic;

namespace EventCentric.EventSourcing
{
    public class State<T> : EventSourced<T>,
        IUpdatesWhen<AnInvalidOperationExceptionOccurred>,
        IUpdatesWhen<Event>,
        IUpdatesAfterSending<Command>
        where T : class, IEventSourced
    {
        public State(Guid id) : base(id) { }

        public State(Guid id, ISnapshot snapshot) : base(id, snapshot) { }

        public State(Guid id, IEnumerable<IEvent> streamOfEvents) : base(id, streamOfEvents) { }

        public T Apply(Message message) => base.UpdateFromMessage(message);

        public T Apply(params Message[] messages) => this.Apply(messages);

        public T Apply(IEnumerable<Message> messages)
        {
            messages.ForEach(m => base.UpdateFromMessage(m));
            return this as T;
        }

        public T Update(Event @event) => base.UpdateFromMessage(@event);

        public T Update(params Event[] events) => this.Update(events);

        public T Update(IEnumerable<Event> events)
        {
            events.ForEach(e => base.UpdateFromMessage(e));
            return this as T;
        }

        public T UpdateAfterSending(Command command) => base.UpdateFromMessage(command);

        public T UpdateAfterSending(params Command[] commands) => this.UpdateAfterSending(commands);

        public T UpdateAfterSending(IEnumerable<Command> commands)
        {
            commands.ForEach(c => base.UpdateFromMessage(c));
            return this as T;
        }

        public T UpdateIf(bool condition, params Event[] events)
        {
            if (condition)
                this.Update(events);
            return this as T;
        }

        public T UpdateAfterSendingIf(bool condition, params Command[] commands)
        {
            if (condition)
                this.UpdateAfterSending(commands);
            return this as T;
        }

        public T ApplyIf(bool condition, params Message[] messages)
        {
            if (condition)
                this.Apply(messages);
            return this as T;
        }

        public T Throw(string message) => this.Update(new AnInvalidOperationExceptionOccurred(message));

        public virtual void When(AnInvalidOperationExceptionOccurred e) { }

        public void When(Event e) { }

        public void AfterSending(Command c) { }
    }
}
