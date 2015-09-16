using EasyTrade.EmpresasQueue.DTOs;
using System;

namespace EasyTrade.EmpresasQueue
{
    public interface IEmpresasQueueApp
    {
        Guid NuevaEmpresa(NuevaEmpresaDto dto);

        Guid DesactivarEmpresa(Guid idEmpresa);

        Guid ReactivarEmpresa(Guid idEmpresa);

        Guid ActualizarEmpresa(EmpresaDto dto);
    }
}