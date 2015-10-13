using EventCentric.EventSourcing;
using InformesDeServicio.Messages.Publicadores.DTOs;

namespace InformesDeServicio.Messages.Publicadores.Store.Events
{
    public class PublicadorRegistrado : Event
    {
        public PublicadorRegistrado(DatosDePublicador datos)
        {
            this.Datos = datos;
        }

        public DatosDePublicador Datos { get; private set; }
    }
}
