using EventCentric.Processing;
using InformesDeServicio.Messages.Publicadores.InProcess.Commands;

namespace InformesDeServicio.Messages.Publicadores.InProcess
{
    public interface IPublicadorInProcessMessageSubscriber :
        ISubscribedTo<RegistrarPublicador>,
        ISubscribedTo<ActualizarDatosDePublicador>,
        ISubscribedTo<DarDeBajaAPublicador>,
        ISubscribedTo<VolverADarDeAltaAPublicador>
    { }
}
