using EasyTrade.EmpresasQueue.DTOs;
using EasyTrade.EmpresasQueue.Especificaciones;
using EasyTrade.Events;
using EasyTrade.Events.EmpresasQueue;
using EasyTrade.Events.EmpresasQueue.DTOs;
using EventCentric;
using EventCentric.EventSourcing;
using EventCentric.Queueing;
using EventCentric.Utils;
using System;
using System.Linq;

namespace EasyTrade.EmpresasQueue
{
    public class EmpresasQueueApp : CrudQueueApplicationService, IEmpresasQueueApp
    {
        public EmpresasQueueApp(ICrudEventQueue queue, IGuidProvider guid, ITimeProvider time)
            : base(queue, guid, time)
        { }

        public Guid ActualizarEmpresa(EmpresaDto dto)
        {
            var transactionId = this.guid.NewGuid();

            var empresa = new Empresa(dto.IdEmpresa, dto.Nombre, dto.Ruc, dto.Descripcion);

            this.queue.Enqueue<EmpresasQueueDbContext>(
                new DatosDeEmpresaActualizados(empresa, time.Now)
                    .FormatAsEventToBeQueued(transactionId, dto.IdEmpresa),
                context =>
                {
                    AlActualizarDatosDeEmpresa.EstaDebeHaberSidoRegistrada(context, empresa.IdEmpresa);
                    AlActualizarDatosDeEmpresa.ElNombreActualizadoDebeSerUnico(context, empresa.Nombre, dto.IdEmpresa);

                    var empresaEntity = context.Empresas.Single(e => e.IdEmpresa == dto.IdEmpresa);
                    empresaEntity.Nombre = dto.Nombre;
                });

            return transactionId;
        }

        public Guid DesactivarEmpresa(Guid idEmpresa)
        {
            var transactionId = this.guid.NewGuid();

            var @event = new EmpresaDesactivada(idEmpresa).FormatAsEventToBeQueued(transactionId, idEmpresa);

            this.queue.Enqueue<EmpresasQueueDbContext>(@event,
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

            this.queue.Enqueue<EmpresasQueueDbContext>(
                new NuevaEmpresaRegistrada(empresa, time.Now).FormatAsEventToBeQueued(transactionId, empresa.IdEmpresa),
                context =>
                {
                    AlRegistrarNuevaEmpresa.ElNombreDebeSerUnico(context, empresa.Nombre);

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

            var @event = new EmpresaReactivada(idEmpresa).FormatAsEventToBeQueued(transactionId, idEmpresa);

            this.queue.Enqueue<EmpresasQueueDbContext>(@event,
                context =>
                {
                    AlReactivarEmpresa.EstaDebeHaberSidoRegistrada(context, idEmpresa);
                });

            return transactionId;
        }
    }
}
