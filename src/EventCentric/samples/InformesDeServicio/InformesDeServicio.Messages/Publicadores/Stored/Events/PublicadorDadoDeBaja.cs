using EventCentric.EventSourcing;
using System;

namespace InformesDeServicio.Messages.Publicadores.Stored.Events
{
    public class PublicadorDadoDeBaja : Event
    {
        public PublicadorDadoDeBaja(DateTime fechaDeBaja)
        {
            this.FechaDeBaja = fechaDeBaja;
        }

        public DateTime FechaDeBaja { get; set; }
    }
}
