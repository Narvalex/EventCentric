using System;
using InformesDeServicio.Publicadores.DTOs;

namespace InformesDeServicio.Publicadores
{
    public interface IPublicadorApp
    {
        Guid ActualizarDatosDePublicador(RegistrarOActualizarPublicadorDto dto);
        Guid DarDeBajaAPublicador(Guid idPublicador);
        Guid RegistrarPublicador(RegistrarOActualizarPublicadorDto dto);
        Guid VolverADarDeAltaAPublicador(Guid idPublicador);
    }
}