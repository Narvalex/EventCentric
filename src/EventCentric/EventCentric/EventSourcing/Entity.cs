using System;

namespace EventCentric.EventSourcing
{
    public abstract class Entity<T> : IMementoOriginator<T> where T : IMemento
    {
        public abstract T SaveToMemento();

        protected Entity(Guid id, Action<Event> update, T memento) // to recall to bring the memento in construction time
        {
            this.Id = id;
            this.Update = update;
        }

        protected Guid Id { get; }

        protected Action<Event> Update { get; }
    }
}
