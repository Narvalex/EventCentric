using EventCentric.EventSourcing;
using InformesDeServicio.Messages.Publicadores.InProcess.Commands;
using InformesDeServicio.Messages.Publicadores.Store.Events;
using System;
using System.Collections.Generic;

namespace InformesDeServicio.Publicadores
{
    public class Publicador : EventSourced,
        IHandles<RegistrarPublicador>,
        IUpdatesOn<PublicadorRegistrado>
    {
        public Publicador(Guid id)
            : base(id)
        { }

        public Publicador(Guid id, IMemento memento)
            : base(id, memento)
        { }

        public Publicador(Guid id, IEnumerable<IEvent> streamOfEvents)
            : base(id, streamOfEvents)
        { }

        public void Handle(RegistrarPublicador command)
        {
            base.Update(
                new PublicadorRegistrado(command.Datos));
        }

        public void On(PublicadorRegistrado e)
        { }
    }
}
