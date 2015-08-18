using Clientes.CommandProcessor.Commands;
using Clientes.Events;
using EventCentric.EventSourcing;
using System;

namespace Clientes.CommandProcessor.Processor
{
    public class ClientesYSaldos : EventSourced,
        IHandle<AgregarCliente>,
        IHandle<ClienteRegistrado>,
        IUpdateOn<ClienteRegistrado>,
        IUpdateOn<CuentaCreadaANuevoCliente>
    {
        public int saldo = 0;

        public ClientesYSaldos(Guid id)
            : base(id)
        { }

        public ClientesYSaldos(Guid id, IMemento memento)
            : base(id, memento)
        { }

        public void Handle(AgregarCliente e)
        {
            base.Publish(new ClienteRegistrado
            {
                IdCliente = e.EventId,
                Nombre = e.Nombre
            });
        }

        public void Handle(ClienteRegistrado e)
        {
            base.Publish(new CuentaCreadaANuevoCliente
            {
                IdCliente = e.EventId,
                Nombre = e.Nombre
            });
        }

        public void On(ClienteRegistrado e)
        { }

        public void On(CuentaCreadaANuevoCliente e)
        { }
    }
}
