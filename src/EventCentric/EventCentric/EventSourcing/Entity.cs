using System;

namespace EventCentric.EventSourcing
{
    public abstract class Entity<T> : IMementoOriginator<T> where T : IMemento
    {
        public abstract T SaveToMemento();

        protected Entity(Guid id, Action<Event> update)
        {
            this.Id = id;
            this.Update = update;
        }

        protected Guid Id { get; }

        protected Action<Event> Update { get; }

        protected void Throw(string message) => this.Update(new AnInvalidOperationExceptionOccurred(message));
    }
}
