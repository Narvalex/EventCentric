using EventCentric.Repository;
using System;
using System.Collections.Generic;

namespace EventCentric.EventSourcing
{
    public abstract class Denormalizer<T> : EventSourced, IDenormalizer
        where T : IEventStoreDbContext
    {
        private Action<T> denormalize = context => { };

        protected Denormalizer(Guid id)
            : base(id)
        { }

        protected Denormalizer(Guid id, IEnumerable<IEvent> streamOfEvents)
            : base(id, streamOfEvents)
        { }

        protected Denormalizer(Guid id, ISnapshot memento)
            : base(id, memento)
        { }

        public void Denormalize(IEventStoreDbContext context)
        {
            this.denormalize((T)context);
        }

        public void UpdateReadModel(Action<T> denormalize)
        {
            this.denormalize = denormalize;
            base.Update(new ReadModelUpdated());
        }

        public void On(ReadModelUpdated e)
        { }
    }
}
