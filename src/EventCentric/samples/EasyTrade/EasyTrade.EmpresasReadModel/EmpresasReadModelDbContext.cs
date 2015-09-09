using EventCentric.Repository;
using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;

namespace EasyTrade.EmpresasReadModel
{
    public class EmpresasReadModelDbContext : EventuallyConsistentDbContext
    {
        public EmpresasReadModelDbContext(TimeSpan timeout, bool isReadOnly, string nameOrconnectionString)
            : base(timeout, isReadOnly, nameOrconnectionString)
        { }

        public IDbSet<EmpresaView> Empresas { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Configurations.Add(new EmpresaViewMap());
        }
    }

    public class EmpresaView
    {
        public Guid IdEmpresa { get; set; }
        public string Nombre { get; set; }
        public string Ruc { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }

    public class EmpresaViewMap : EntityTypeConfiguration<EmpresaView>
    {
        public EmpresaViewMap()
        {
            // Primary Key
            this.HasKey(t => t.IdEmpresa);

            // Table & Column Mappings
            this.Property(t => t.IdEmpresa).HasColumnName("IdEmpresa");
            this.Property(t => t.Nombre).HasColumnName("Nombre");
            this.Property(t => t.Ruc).HasColumnName("Ruc");
            this.Property(t => t.Descripcion).HasColumnName("Descripcion");
            this.Property(t => t.FechaRegistro).HasColumnName("FechaRegistro");
            this.Property(t => t.FechaActualizacion).HasColumnName("FechaActualizacion");
        }
    }
}
