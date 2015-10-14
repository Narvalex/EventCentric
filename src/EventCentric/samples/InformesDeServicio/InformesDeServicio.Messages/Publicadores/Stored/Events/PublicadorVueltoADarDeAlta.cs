using EventCentric.EventSourcing;
using System;

namespace InformesDeServicio.Messages.Publicadores.Stored.Events
{
    public class PublicadorVueltoADarDeAlta : Event
    {
        public PublicadorVueltoADarDeAlta(DateTime fechaDeVueltaADarDeAlta)
        {
            this.FechaDeVueltaADarDeAlta = fechaDeVueltaADarDeAlta;
        }

        public DateTime FechaDeVueltaADarDeAlta { get; private set; }
    }
}
