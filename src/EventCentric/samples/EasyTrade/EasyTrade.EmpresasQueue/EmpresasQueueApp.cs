using EasyTrade.EmpresasQueue.DTOs;
using EasyTrade.Events;
using EasyTrade.Events.EmpresasQueue.DTOs;
using EventCentric;
using EventCentric.Queueing;
using EventCentric.Utils;
using System;

namespace EasyTrade.EmpresasQueue
{
    public class EmpresasQueueApp : CrudApplicationService, IEmpresasQueueApp
    {
        public EmpresasQueueApp(ICrudEventBus bus, IGuidProvider guid)
            : base(bus, guid)
        { }

        public Guid NuevaEmpresa(NuevaEmpresaDto dto)
        {
            var transactionId = this.guid.NewGuid;
            var empresa = new Empresa(transactionId, dto.Nombre, dto.Ruc, dto.Descripcion);

            this.bus.Send<EmpresasQueueDbContext>(
                new NuevaEmpresaRegistrada(empresa.IdEmpresa, transactionId, empresa),
                context =>
                {
                    // Seria bueno verificar primero si ya existe la empresa en al base de datos.
                    context.Empresas.Add(new EmpresaEntity
                    {
                        IdEmpresa = empresa.IdEmpresa,
                        Nombre = empresa.Nombre
                    });
                });

            return transactionId;
        }
    }
}
