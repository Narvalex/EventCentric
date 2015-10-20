namespace InformesDeServicio.Messages.Publicadores.DTOs
{
    public class DatosDePublicador
    {
        public DatosDePublicador(string nombres, string apellidos)
        {
            this.Nombres = nombres;
            this.Apellidos = apellidos;
        }

        public string Nombres { get; private set; }
        public string Apellidos { get; private set; }
    }
}
