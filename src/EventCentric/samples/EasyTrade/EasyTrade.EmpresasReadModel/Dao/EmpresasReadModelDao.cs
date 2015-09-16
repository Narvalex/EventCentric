using EventCentric.Repository.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTrade.EmpresasReadModel.Dao
{
    public class EmpresasReadModelDao : IEmpresasReadModelDao
    {
        private readonly Func<EmpresasReadModelDbContext> contextFactory;

        public EmpresasReadModelDao(Func<EmpresasReadModelDbContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        public List<EmpresaEntity> ObtenerListaDeTodasLasEmpresas()
        {
            using (var context = this.contextFactory())
            {
                return context.Empresas.ToList();
            }
        }

        public EventuallyConsistentResult AwaitResult(Guid idTransaction)
        {
            using (var context = this.contextFactory())
            {
                return context.AwaitEventualConsistency(idTransaction);
            }
        }

        public EmpresaEntity ObtenerEmpresa(Guid idEmpresa)
        {
            using (var context = this.contextFactory())
            {
                return context.Empresas.FirstOrDefault(e => e.IdEmpresa == idEmpresa);
            }
        }
    }
}
