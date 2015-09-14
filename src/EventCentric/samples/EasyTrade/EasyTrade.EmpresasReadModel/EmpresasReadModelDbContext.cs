using EventCentric.Repository;
using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;

namespace EasyTrade.EmpresasReadModel
{
    public class EmpresasReadModelDbContext : EventuallyConsistentDbContext
    {
        public EmpresasReadModelDbContext(TimeSpan timeout, bool isReadOnly, string nameOrConnectionString)
            : base(timeout, isReadOnly, nameOrConnectionString)
        { }

        public EmpresasReadModelDbContext(bool isReadOnly, string nameOrConnectionString)
            : this(TimeSpan.FromSeconds(30), isReadOnly, nameOrConnectionString)
        { }

        public IDbSet<EmpresaEntity> Empresas { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Configurations.Add(new EmpresaEntityMap());
        }
    }

    public class EmpresaEntity
    {
        public Guid IdEmpresa { get; set; }
        public string Nombre { get; set; }
        public string Ruc { get; set; }
        public string Descripcion { get; set; }
        public bool Activada { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }

    public class EmpresaEntityMap : EntityTypeConfiguration<EmpresaEntity>
    {
        public EmpresaEntityMap()
        {
            // Primary Key
            this.HasKey(t => t.IdEmpresa);

            // Table & Column Mappings
            this.ToTable("Empresas", "ReadModel");
            this.Property(t => t.IdEmpresa).HasColumnName("IdEmpresa");
            this.Property(t => t.Nombre).HasColumnName("Nombre");
            this.Property(t => t.Ruc).HasColumnName("Ruc");
            this.Property(t => t.Descripcion).HasColumnName("Descripcion");
            this.Property(t => t.Activada).HasColumnName("Activada");
            this.Property(t => t.FechaRegistro).HasColumnName("FechaRegistro");
            this.Property(t => t.FechaActualizacion).HasColumnName("FechaActualizacion");
        }
    }
}
