using Clientes.Events;
using EventCentric.EventSourcing;
using System;

namespace Clientes.ReadModel
{
    public class ClientesDenormalizer : Denormalizer<ClientesReadModelDbContext>,
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
            base.UpdateReadModel(context =>
            {
                context.Clientes.Add(new Cliente
                {
                    Id = e.IdCliente,
                    Nombre = e.Nombre,
                    // This is a bad practice
                    FechaIngreso = DateTime.Now
                });
            });
        }
    }
}
