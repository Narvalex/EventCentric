using EventCentric.Repository;
using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;

namespace EasyTrade.EmpresasQueue
{
    public class EmpresasQueueDbContext : EventQueueDbContext, IEventQueueDbContext
    {
        public EmpresasQueueDbContext(bool isReadOnly, string nameOrconnectionString)
            : base(isReadOnly, nameOrconnectionString)
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
    }

    public class EmpresaEntityMap : EntityTypeConfiguration<EmpresaEntity>
    {
        public EmpresaEntityMap()
        {
            // Primary Key
            this.HasKey(t => t.Nombre);

            // Table & Column Mappings
            this.ToTable("Empresas", "SetValidation");
            this.Property(t => t.IdEmpresa).HasColumnName("IdEmpresa");
            this.Property(t => t.Nombre).HasColumnName("Nombre");
        }
    }
}
