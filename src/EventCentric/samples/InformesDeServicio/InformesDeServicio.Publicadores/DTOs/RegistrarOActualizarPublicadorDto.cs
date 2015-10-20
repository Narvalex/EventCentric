using System;

namespace InformesDeServicio.Publicadores.DTOs
{
    public class RegistrarOActualizarPublicadorDto
    {
        public Guid IdPublicador { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
    }
}
