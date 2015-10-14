using EventCentric.EventSourcing;
using System;

namespace InformesDeServicio.Messages.Publicadores.InProcess.Commands
{
    public class DarDeBajaAPublicador : Event
    {
        public DarDeBajaAPublicador(Guid idPublicador, DateTime fechaDeBaja)
        {
            this.IdPublicador = idPublicador;
            this.FechaDeBaja = fechaDeBaja;
        }

        public Guid IdPublicador { get; private set; }
        public DateTime FechaDeBaja { get; private set; }
    }
}
