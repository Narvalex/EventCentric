using EventCentric.Repository;
using System;

namespace EventCentric.EventSourcing
{
    public abstract class Denormalizer : EventSourced, IDenormalizer
    {
        protected Action<IEventStoreDbContext> denormalize = context => { };

        protected Denormalizer(Guid id)
            : base(id)
        { }

        protected Denormalizer(Guid id, IMemento memento)
            : base(id, memento)
        { }

        public void Denormalize(IEventStoreDbContext context)
        {
            this.denormalize(context);
        }
    }
}
