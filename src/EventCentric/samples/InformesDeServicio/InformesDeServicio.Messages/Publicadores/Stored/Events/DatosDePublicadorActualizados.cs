using EventCentric.EventSourcing;
using InformesDeServicio.Messages.Publicadores.DTOs;

namespace InformesDeServicio.Messages.Publicadores.Stored.Events
{
    public class DatosDePublicadorActualizados : Event
    {
        public DatosDePublicadorActualizados(DatosDePublicador datosActualizados)
        {
            this.DatosActualizados = datosActualizados;
        }

        public DatosDePublicador DatosActualizados { get; private set; }
    }
}
