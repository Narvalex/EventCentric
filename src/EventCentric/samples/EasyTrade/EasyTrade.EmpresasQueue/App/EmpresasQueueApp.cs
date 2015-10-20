using EasyTrade.EmpresasQueue.DTOs;
using EasyTrade.EmpresasQueue.Especificaciones;
using EasyTrade.Events;
using EasyTrade.Events.EmpresasQueue;
using EasyTrade.Events.EmpresasQueue.DTOs;
using EventCentric.Messaging;
using EventCentric.Node;
using EventCentric.Utils;
using System;
using System.Linq;

namespace EasyTrade.EmpresasQueue
{
    public class EmpresasQueueApp : CrudApplicationService, IEmpresasQueueApp
    {
        public EmpresasQueueApp(ICrudEventBus bus, IGuidProvider guid, ITimeProvider time)
            : base(bus, guid, time)
        { }

        public Guid ActualizarEmpresa(EmpresaDto dto)
        {
            var transactionId = this.guid.NewGuid();

            var empresa = new Empresa(dto.IdEmpresa, dto.Nombre, dto.Ruc, dto.Descripcion);

            this.bus.Publish<EmpresasQueueDbContext>(transactionId, dto.IdEmpresa,
                new DatosDeEmpresaActualizados(empresa, time.Now),
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

            var @event = new EmpresaDesactivada(idEmpresa);

            this.bus.Publish<EmpresasQueueDbContext>(
                transactionId,
                idEmpresa,
                @event,
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

            this.bus.Publish<EmpresasQueueDbContext>(transactionId, empresa.IdEmpresa,
                new NuevaEmpresaRegistrada(empresa, time.Now),
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

            var @event = new EmpresaReactivada(idEmpresa);

            this.bus.Publish<EmpresasQueueDbContext>(transactionId, idEmpresa, @event,
                context =>
                {
                    AlReactivarEmpresa.EstaDebeHaberSidoRegistrada(context, idEmpresa);
                });

            return transactionId;
        }
    }
}
