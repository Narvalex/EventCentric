using EventCentric.EventSourcing;
using System;

namespace InformesDeServicio.Messages.Publicadores.Stored.Events
{
    public class SeIntentoVolverADarDeAltaAPublicadorQueYaEstaDadoDeAlta : Event
    {
        public SeIntentoVolverADarDeAltaAPublicadorQueYaEstaDadoDeAlta(DateTime fechaDeIntento)
        {
            this.FechaDeIntento = fechaDeIntento;
        }

        public DateTime FechaDeIntento { get; private set; }
    }
}
