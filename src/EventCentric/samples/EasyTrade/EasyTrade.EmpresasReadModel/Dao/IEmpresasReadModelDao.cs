using System;
using System.Collections.Generic;
using EventCentric.Repository.Mapping;

namespace EasyTrade.EmpresasReadModel.Dao
{
    public interface IEmpresasReadModelDao
    {
        EventuallyConsistentResult AwaitResult(Guid transactionId);
        List<EmpresaEntity> ObtenerListaDeTodasLasEmpresas();
    }
}