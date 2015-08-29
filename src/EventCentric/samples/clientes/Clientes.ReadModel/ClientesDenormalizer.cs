using Clientes.Events;
using EventCentric.Database;
using EventCentric.EventSourcing;
using System;
using System.Linq;

namespace Clientes.ReadModel
{
    public class ClientesDenormalizer : Denormalizer<ClientesReadModelDbContext>,
        IHandles<CuentaCreadaANuevoCliente>,
        IHandles<SolicitudNuevoClienteRecibida>
    {
        public ClientesDenormalizer(Guid id)
            : base(id)
        { }

        public ClientesDenormalizer(Guid id, IMemento memento)
            : base(id, memento)
        { }

        public void Handle(SolicitudNuevoClienteRecibida e)
        {
            base.UpdateReadModel(context =>
            {
                context.AddOrUpdate(
                    () => context.Clientes.Where(c => c.Id == e.IdCliente).FirstOrDefault(),
                    () => new Cliente
                    {
                        Id = e.IdCliente,
                        Nombre = e.Nombre,
                        // This is a bad practice
                        FechaIngreso = null,
                        FechaSolicitud = DateTime.Now
                    },
                    cliente => cliente.FechaSolicitud = DateTime.Now);
            });
        }

        public void Handle(CuentaCreadaANuevoCliente e)
        {
            base.UpdateReadModel(context =>
            {
                context.AddOrUpdate(
                     () => context.Clientes.Where(c => c.Id == e.IdCliente).FirstOrDefault(),
                     () => new Cliente
                     {
                         Id = e.IdCliente,
                         Nombre = e.Nombre,
                         // This is a bad practice
                         FechaIngreso = DateTime.Now,
                         FechaSolicitud = null
                     },
                     cliente => cliente.FechaIngreso = DateTime.Now);
            });
        }
    }
}
