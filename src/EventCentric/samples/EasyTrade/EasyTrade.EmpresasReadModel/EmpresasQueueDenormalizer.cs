using EasyTrade.Events;
using EasyTrade.Events.EmpresasQueue;
using EventCentric.EventSourcing;
using EventCentric.Repository.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTrade.EmpresasReadModel
{
    public class EmpresasQueueDenormalizer : Denormalizer<EmpresasReadModelDbContext>,
        IHandles<NuevaEmpresaRegistrada>,
        IHandles<EmpresaDesactivada>,
        IHandles<EmpresaReactivada>,
        IHandles<DatosDeEmpresaActualizados>
    {
        public EmpresasQueueDenormalizer(Guid id)
            : base(id)
        { }

        public EmpresasQueueDenormalizer(Guid id, IEnumerable<IEvent> streamOfEvents)
            : base(id, streamOfEvents)
        { }

        public EmpresasQueueDenormalizer(Guid id, IMemento memento)
            : base(id, memento)
        { }

        public void Handle(EmpresaReactivada e)
        {
            base.UpdateReadModel(context =>
            {
                var empresa = context.Empresas.Single(x => x.IdEmpresa == e.IdEmpresa);
                empresa.Activada = true;

                var consistencyResult = new EventuallyConsistentResult
                {
                    ResultType = 1,
                    TransactionId = e.TransactionId,
                    Message = string.Format("La empresa {0} ha sido reactivada exitosamente", empresa.Nombre)
                };

                context.EventuallyConsistentResults.Add(consistencyResult);
            });
        }

        public void Handle(DatosDeEmpresaActualizados e)
        {
            base.UpdateReadModel(context =>
            {
                var empresa = context.Empresas.Single(x => x.IdEmpresa == e.Empresa.IdEmpresa);
                empresa.Nombre = e.Empresa.Nombre;
                empresa.Ruc = e.Empresa.Ruc;
                empresa.Descripcion = e.Empresa.Descripcion;

                var consistencyResult = new EventuallyConsistentResult
                {
                    ResultType = 1,
                    TransactionId = e.TransactionId,
                    Message = string.Format("La empresa {0} ha sido actualizada exitosamente", empresa.Nombre)
                };

                context.EventuallyConsistentResults.Add(consistencyResult);
            });
        }

        public void Handle(EmpresaDesactivada e)
        {
            base.UpdateReadModel(context =>
            {
                var empresa = context.Empresas.Single(x => x.IdEmpresa == e.IdEmpresa);
                empresa.Activada = false;

                var consistencyResult = new EventuallyConsistentResult
                {
                    ResultType = 1,
                    TransactionId = e.TransactionId,
                    Message = string.Format("La empresa {0} ha sido desactivada exitosamente", empresa.Nombre)
                };

                context.EventuallyConsistentResults.Add(consistencyResult);
            });
        }

        public void Handle(NuevaEmpresaRegistrada e)
        {
            base.UpdateReadModel(context =>
            {
                var empresa = new EmpresaEntity
                {
                    IdEmpresa = e.Empresa.IdEmpresa,
                    Nombre = e.Empresa.Nombre,
                    Descripcion = e.Empresa.Descripcion,
                    Ruc = e.Empresa.Ruc,
                    Activada = true,
                    FechaRegistro = e.FechaRegistro,
                    FechaActualizacion = e.FechaRegistro
                };

                var consistencyResult = new EventuallyConsistentResult
                {
                    ResultType = 1,
                    TransactionId = e.TransactionId,
                    Message = string.Format("La empresa {0} ha sido registrada exitosamente", e.Empresa.Nombre)
                };

                context.Empresas.Add(empresa);
                context.EventuallyConsistentResults.Add(consistencyResult);
            });
        }
    }
}
