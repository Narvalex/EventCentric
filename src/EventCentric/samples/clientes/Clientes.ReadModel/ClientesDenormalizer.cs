using Clientes.Events;
using EventCentric.EventSourcing;
using System;

namespace Clientes.ReadModel
{
    public class ClientesDenormalizer : Denormalizer,
        IHandles<CuentaCreadaANuevoCliente>
    {
        public ClientesDenormalizer(Guid id)
            : base(id)
        { }

        public ClientesDenormalizer(Guid id, IMemento memento)
            : base(id, memento)
        { }

        public void Handle(CuentaCreadaANuevoCliente e)
        {

        }
    }
}
