using EventCentric.EventSourcing;
using System;
using System.Collections.Generic;

namespace InformesDeServicio.Publicador
{
    public class PublicadorAggregate : EventSourced
    {
        public PublicadorAggregate(Guid id)
            : base(id)
        {

        }

        public PublicadorAggregate(Guid id, IMemento memento)
            : base(id, memento)
        {

        }

        public PublicadorAggregate(Guid id, IEnumerable<IEvent> streamOfEvents)
            : base(id, streamOfEvents)
        {

        }
    }
}
