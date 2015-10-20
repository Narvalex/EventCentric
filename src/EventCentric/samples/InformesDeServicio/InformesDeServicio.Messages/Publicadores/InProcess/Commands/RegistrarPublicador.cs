using EventCentric.EventSourcing;
using InformesDeServicio.Messages.Publicadores.DTOs;
using System;

namespace InformesDeServicio.Messages.Publicadores.InProcess.Commands
{
    public class RegistrarPublicador : Event
    {
        public RegistrarPublicador(Guid idPublicador, DatosDePublicador datos, DateTime fechaRegistro)
        {
            this.IdPublicador = idPublicador;
            this.Datos = datos;
            this.FechaRegistro = fechaRegistro;
        }

        public Guid IdPublicador { get; private set; }
        public DatosDePublicador Datos { get; private set; }
        public DateTime FechaRegistro { get; private set; }
    }
}

