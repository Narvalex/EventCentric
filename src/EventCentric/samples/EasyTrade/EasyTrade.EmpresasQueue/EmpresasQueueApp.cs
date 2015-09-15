using EasyTrade.EmpresasQueue.DTOs;
using EasyTrade.EmpresasQueue.Especificaciones;
using EasyTrade.Events;
using EasyTrade.Events.EmpresasQueue;
using EasyTrade.Events.EmpresasQueue.DTOs;
using EventCentric;
using EventCentric.Queueing;
using EventCentric.Utils;
using System;

namespace EasyTrade.EmpresasQueue
{
    public class EmpresasQueueApp : CrudApplicationService, IEmpresasQueueApp
    {
        public EmpresasQueueApp(ICrudEventBus bus, IGuidProvider guid, ITimeProvider time)
            : base(bus, guid, time)
        { }

        public Guid DesactivarEmpresa(Guid idEmpresa)
        {
            var transactionId = this.guid.NewGuid();

            var @event = new EmpresaDesactivada(idEmpresa, transactionId, idEmpresa);

            this.bus.Send<EmpresasQueueDbContext>(@event,
                context =>
                {
                    AlDesactivarEmpresa.EstaDebeHaberSidoRegistrada(context, idEmpresa);
                });

            return transactionId;
        }

        public Guid NuevaEmpresa(NuevaEmpresaDto dto)
        {
            var transactionId = this.guid.NewGuid();
            var empresa = new Empresa(transactionId, dto.Nombre, dto.Ruc, dto.Descripcion);

            this.bus.Send<EmpresasQueueDbContext>(
                new NuevaEmpresaRegistrada(empresa.IdEmpresa, transactionId, empresa, time.Now),
                context =>
                {
                    AlRegistrarNuevaEmpresa.ElNombreDebeSerUnico(context, empresa.Nombre);

                    // Seria bueno verificar primero si ya existe la empresa en al base de datos.
                    context.Empresas.Add(new EmpresaEntity
                    {
                        IdEmpresa = empresa.IdEmpresa,
                        Nombre = empresa.Nombre
                    });
                });

            return transactionId;
        }

        public Guid ReactivarEmpresa(Guid idEmpresa)
        {
            var transactionId = this.guid.NewGuid();

            var @event = new EmpresaReactivada(idEmpresa, transactionId, idEmpresa);

            this.bus.Send<EmpresasQueueDbContext>(@event,
                context =>
                {
                    AlReactivarEmpresa.EstaDebeHaberSidoRegistrada(context, idEmpresa);
                });

            return transactionId;
        }
    }
}
