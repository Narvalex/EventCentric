using Clientes.Events;
using EventCentric.EventSourcing;
using System;

namespace Clientes.EventProcessor
{
    public class Clientes : EventSourced,
        IHandles<SolicitudNuevoClienteRecibida>,
        IUpdatesOn<CuentaCreadaANuevoCliente>
    {
        public Clientes(Guid id)
            : base(id)
        { }

        public Clientes(Guid id, IMemento memento)
            : base(id, memento)
        { }

        public void Handle(SolicitudNuevoClienteRecibida e)
        {
            base.Publish(new CuentaCreadaANuevoCliente
            {
                Nombre = e.Nombre,
                IdCliente = e.IdCliente
            });
        }

        public void On(CuentaCreadaANuevoCliente e)
        { }
    }
}
