using System;

namespace InformesDeServicio.Messages.Publicadores.DTOs
{
    public class DatosDePublicador
    {
        public DatosDePublicador(string nombres, string apellidos, DateTime fechaRegistroEnSistema, DateTime fechaActualizacionEnSistema)
        {
            this.Nombres = nombres;
            this.Apellidos = apellidos;
            this.FechaRegistroEnSistema = fechaRegistroEnSistema;
            this.FechaActualizacionEnSistema = fechaActualizacionEnSistema;
        }

        public string Nombres { get; private set; }
        public string Apellidos { get; private set; }
        public DateTime FechaRegistroEnSistema { get; private set; }
        public DateTime FechaActualizacionEnSistema { get; private set; }
    }
}
