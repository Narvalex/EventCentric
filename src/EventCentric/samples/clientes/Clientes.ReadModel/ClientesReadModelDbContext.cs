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

        public DateTime? FechaIngreso { get; set; }

        public DateTime? FechaSolicitud { get; set; }
    }

    //    CREATE TABLE[dbo].[Clientes](
    //	[Id]
    //    [uniqueidentifier]
    //    NOT NULL,
    //    [Nombre] [nvarchar](max) NULL,
    //	[FechaIngreso]
    //    [datetime]
    //    NULL,
    //	[FechaSolicitud]
    //    [datetime]
    //    NULL,
    //PRIMARY KEY CLUSTERED
    //(
    //    [Id] ASC
    //)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
    //) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]

}
