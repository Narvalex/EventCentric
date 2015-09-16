using EventCentric.Repository.Mapping;
using System;
using System.Collections.Generic;

namespace EasyTrade.EmpresasReadModel.Dao
{
    public interface IEmpresasReadModelDao
    {
        EventuallyConsistentResult AwaitResult(Guid transactionId);
        List<EmpresaEntity> ObtenerListaDeTodasLasEmpresas();
        EmpresaEntity ObtenerEmpresa(Guid idEmpresa);
    }
}