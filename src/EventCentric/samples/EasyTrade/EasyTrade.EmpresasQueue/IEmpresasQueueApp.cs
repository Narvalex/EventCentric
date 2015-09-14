using EasyTrade.EmpresasQueue.DTOs;
using System;

namespace EasyTrade.EmpresasQueue
{
    public interface IEmpresasQueueApp
    {
        Guid NuevaEmpresa(NuevaEmpresaDto dto);

        Guid DesactivarEmpresa(Guid idEmpresa);
    }
}