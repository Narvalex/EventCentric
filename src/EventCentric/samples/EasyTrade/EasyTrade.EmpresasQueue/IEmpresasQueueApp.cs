using System;
using EasyTrade.EmpresasQueue.DTOs;

namespace EasyTrade.EmpresasQueue
{
    public interface IEmpresasQueueApp
    {
        Guid NuevaEmpresa(NuevaEmpresaDto dto);
    }
}