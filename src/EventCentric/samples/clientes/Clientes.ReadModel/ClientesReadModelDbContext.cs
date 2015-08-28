using EventCentric.Repository;
using System;
using System.Data.Entity;

namespace Clientes.ReadModel
{
    public class ClientesReadModelDbContext : EventStoreDbContext
    {
        public ClientesReadModelDbContext(bool isReadOnly, string nameOrconnectionString)
            : base(isReadOnly, nameOrconnectionString)
        { }

        public ClientesReadModelDbContext()
            : base(false, "Name=defaultConnection")
        { }

        public IDbSet<Cliente> Clientes { get; set; }
    }

    public class Cliente
    {
        public Guid Id { get; set; }

        public string Nombre { get; set; }

        public DateTime FechaIngreso { get; set; }
    }
}
