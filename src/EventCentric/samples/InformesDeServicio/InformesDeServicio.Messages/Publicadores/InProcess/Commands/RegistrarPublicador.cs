﻿using EventCentric.EventSourcing;
using InformesDeServicio.Messages.Publicadores.DTOs;
using System;

namespace InformesDeServicio.Messages.Publicadores.InProcess.Commands
{
    public class RegistrarPublicador : Event
    {
        public RegistrarPublicador(Guid idPublicador, DatosDePublicador datos)
        {
            this.IdPublicador = idPublicador;
            this.Datos = datos;
        }

        public Guid IdPublicador { get; private set; }
        public DatosDePublicador Datos { get; private set; }
    }
}
