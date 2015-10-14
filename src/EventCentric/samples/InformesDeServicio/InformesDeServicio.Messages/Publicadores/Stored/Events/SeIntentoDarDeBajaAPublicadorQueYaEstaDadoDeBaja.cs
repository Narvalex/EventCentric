using EventCentric.EventSourcing;
using System;

namespace InformesDeServicio.Messages.Publicadores.Stored.Events
{
    public class SeIntentoDarDeBajaAPublicadorQueYaEstaDadoDeBaja : Event
    {
        public SeIntentoDarDeBajaAPublicadorQueYaEstaDadoDeBaja(DateTime fechaDeIntento)
        {
            this.FechaDeIntento = fechaDeIntento;
        }

        public DateTime FechaDeIntento { get; private set; }
    }
}
