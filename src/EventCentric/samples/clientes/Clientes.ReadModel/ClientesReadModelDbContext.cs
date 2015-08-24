using EventCentric.Repository;
using System;
using System.Data.Entity;

namespace Clientes.ReadModel
{
    public class ClientesReadModelDbContext : EventStoreDbContext
    {
        public ClientesReadModelDbContext(string nameOrconnectionString)
            : base(nameOrconnectionString)
        { }

        public ClientesReadModelDbContext()
            : base("Name=defaultConnection")
        { }

        IDbSet<Cliente> Clientes { get; set; }
    }

    public class Cliente
    {
        public Guid Id { get; set; }

        public string Nombre { get; set; }

        public DateTime FechaIngreso { get; set; }
    }
}
