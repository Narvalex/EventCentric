using EventCentric.EventSourcing;
using InformesDeServicio.Messages.Publicadores.DTOs;

namespace InformesDeServicio.Messages.Publicadores.InProcess.Commands
{
    public class RegistrarPublicador : Event
    {
        public RegistrarPublicador(DatosDePublicador datos)
        {
            this.Datos = datos;
        }

        public DatosDePublicador Datos { get; private set; }
    }
}
