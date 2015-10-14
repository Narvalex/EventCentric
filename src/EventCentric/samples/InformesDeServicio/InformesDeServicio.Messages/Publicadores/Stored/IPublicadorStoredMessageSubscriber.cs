using EventCentric.Processing;
using InformesDeServicio.Messages.Publicadores.Stored.Events;

namespace InformesDeServicio.Messages.Publicadores.Stored
{
    public interface IPublicadorStoredMessageSubscriber :
        ISubscribedTo<PublicadorRegistrado>,
        ISubscribedTo<DatosDePublicadorActualizados>,
        ISubscribedTo<PublicadorDadoDeBaja>,
        ISubscribedTo<PublicadorVueltoADarDeAlta>,
        ISubscribedTo<SeIntentoDarDeBajaAPublicadorQueYaEstaDadoDeBaja>,
        ISubscribedTo<SeIntentoVolverADarDeAltaAPublicadorQueYaEstaDadoDeAlta>
    { }
}
