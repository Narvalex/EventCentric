using EventCentric.EventSourcing;
using System;

namespace InformesDeServicio.Messages.Publicadores.InProcess.Commands
{
    public class VolverADarDeAltaAPublicador : Event
    {
        public VolverADarDeAltaAPublicador(Guid idPublicador, DateTime fechaDeVueltaADarDeAlta)
        {
            this.IdPublicador = idPublicador;
            this.FechaDeVueltaADarDeAlta = fechaDeVueltaADarDeAlta;
        }

        public Guid IdPublicador { get; private set; }
        public DateTime FechaDeVueltaADarDeAlta { get; private set; }
    }
}
