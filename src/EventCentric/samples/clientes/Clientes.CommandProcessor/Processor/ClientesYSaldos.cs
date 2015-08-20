using Clientes.Commands;
using Clientes.Events;
using EventCentric.EventSourcing;
using System;

namespace Clientes.CommandProcessor.Processor
{
    public class ClientesYSaldos : EventSourced,
        IHandle<RegistrarNuevoCliente>,
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

        public void Handle(RegistrarNuevoCliente e)
        {
            base.Publish(new ClienteRegistrado
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
