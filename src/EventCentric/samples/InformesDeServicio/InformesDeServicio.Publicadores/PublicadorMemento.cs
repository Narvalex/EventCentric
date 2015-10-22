using EventCentric.EventSourcing;

namespace InformesDeServicio.Publicadores
{
    public class PublicadorMemento : Memento
    {
        public PublicadorMemento(long version, bool publicadorEstaDadoDeAlta)
            : base(version)
        {
            this.PublicadorEstaDadoDeAlta = publicadorEstaDadoDeAlta;
        }

        public bool PublicadorEstaDadoDeAlta { get; private set; }
    }
}
