using EventCentric.EventSourcing;
using InformesDeServicio.Messages.Publicadores.DTOs;
using System;

namespace InformesDeServicio.Messages.Publicadores.InProcess.Commands
{
    public class ActualizarDatosDePublicador : Event
    {
        public ActualizarDatosDePublicador(Guid idPublicador, DatosDePublicador datosActualizados, DateTime fechaActualizacion)
        {
            this.IdPublicador = idPublicador;
            this.DatosActualizados = datosActualizados;
            this.FechaActualizacion = fechaActualizacion;
        }

        public Guid IdPublicador { get; private set; }
        public DatosDePublicador DatosActualizados { get; private set; }
        public DateTime FechaActualizacion { get; private set; }
    }
}
