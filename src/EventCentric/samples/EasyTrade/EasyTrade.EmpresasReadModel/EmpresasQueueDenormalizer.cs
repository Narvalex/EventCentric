using EasyTrade.Events;
using EventCentric.EventSourcing;
using EventCentric.Repository.Mapping;
using System;

namespace EasyTrade.EmpresasReadModel
{
    public class EmpresasQueueDenormalizer : Denormalizer<EmpresasReadModelDbContext>,
        IHandles<NuevaEmpresaRegistrada>
    {
        // esto es innecesario. Siempre sera null. Un denormalizer no deberia ser compatible con mementos complejos
        private string nombre;

        public EmpresasQueueDenormalizer(Guid id)
            : base(id)
        { }

        public EmpresasQueueDenormalizer(Guid id, IMemento memento)
            : base(id, memento)
        {
            var state = (DenormalizerMemento)memento;
            this.nombre = state.Nombre;
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

        public override IMemento SaveToMemento()
        {
            return new DenormalizerMemento(this.Version, this.nombre);
        }

        public class DenormalizerMemento : Memento
        {
            public DenormalizerMemento(int version, string nombre)
                : base(version)
            {
                this.Nombre = nombre;
            }

            public string Nombre { get; private set; }
        }
    }
}
