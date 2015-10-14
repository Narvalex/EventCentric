using EventCentric.EventSourcing;
using InformesDeServicio.Messages.Publicadores.InProcess.Commands;
using InformesDeServicio.Messages.Publicadores.Stored.Events;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace InformesDeServicio.Publicadores
{
    #region +
    [Guid("86669D61-64CD-418D-AEE8-F781F595A975")]
    #endregion
    public class Publicador : EventSourced,
        IHandles<RegistrarPublicador>,
        IHandles<ActualizarDatosDePublicador>,
        IHandles<DarDeBajaAPublicador>,
        IHandles<VolverADarDeAltaAPublicador>,
        IUpdatesOn<PublicadorRegistrado>,
        IUpdatesOn<DatosDePublicadorActualizados>,
        IUpdatesOn<PublicadorDadoDeBaja>,
        IUpdatesOn<PublicadorVueltoADarDeAlta>,
        IUpdatesOn<SeIntentoDarDeBajaAPublicadorQueYaEstaDadoDeBaja>,
        IUpdatesOn<SeIntentoVolverADarDeAltaAPublicadorQueYaEstaDadoDeAlta>
    {
        private bool publicadorEstaDadoDeAlta = false;

        public Publicador(Guid id)
            : base(id)
        { }

        public Publicador(Guid id, IMemento memento)
            : base(id, memento)
        {
            var state = ((PublicadorMemento)memento);

            // make a copy of the state values to avoid concurrency problems with reusing references.
            this.publicadorEstaDadoDeAlta = state.PublicadorEstaDadoDeAlta;
        }

        public Publicador(Guid id, IEnumerable<IEvent> streamOfEvents)
            : base(id, streamOfEvents)
        { }

        public void Handle(RegistrarPublicador command)
        {
            base.Update(
                new PublicadorRegistrado(command.Datos));
        }

        public void Handle(ActualizarDatosDePublicador command)
        {
            base.Update(
                new DatosDePublicadorActualizados(command.DatosActualizados));
        }

        public void Handle(DarDeBajaAPublicador command)
        {
            if (this.publicadorEstaDadoDeAlta)
                base.Update(new PublicadorDadoDeBaja(command.FechaDeBaja));
            else
                base.Update(new SeIntentoDarDeBajaAPublicadorQueYaEstaDadoDeBaja(command.FechaDeBaja));
        }

        public void Handle(VolverADarDeAltaAPublicador command)
        {
            if (this.publicadorEstaDadoDeAlta)
                base.Update(new SeIntentoVolverADarDeAltaAPublicadorQueYaEstaDadoDeAlta(command.FechaDeVueltaADarDeAlta));
            else
                base.Update(new PublicadorVueltoADarDeAlta(command.FechaDeVueltaADarDeAlta));
        }

        public void On(PublicadorRegistrado e)
        {
            this.publicadorEstaDadoDeAlta = true;
        }

        public void On(DatosDePublicadorActualizados e)
        { }

        public void On(PublicadorDadoDeBaja e)
        {
            this.publicadorEstaDadoDeAlta = false;
        }

        public void On(PublicadorVueltoADarDeAlta e)
        {
            this.publicadorEstaDadoDeAlta = true;
        }

        public void On(SeIntentoDarDeBajaAPublicadorQueYaEstaDadoDeBaja e)
        { }

        public void On(SeIntentoVolverADarDeAltaAPublicadorQueYaEstaDadoDeAlta e)
        { }

        public override IMemento SaveToMemento()
        {
            return new PublicadorMemento(this.Version, this.publicadorEstaDadoDeAlta);
        }
    }

    public class PublicadorMemento : Memento
    {
        public PublicadorMemento(int version, bool publicadorEstaDadoDeAlta)
            : base(version)
        {
            this.PublicadorEstaDadoDeAlta = publicadorEstaDadoDeAlta;
        }

        public bool PublicadorEstaDadoDeAlta { get; private set; }
    }
}
