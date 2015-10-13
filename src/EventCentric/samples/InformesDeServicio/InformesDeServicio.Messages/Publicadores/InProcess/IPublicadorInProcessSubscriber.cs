using EventCentric.Processing;
using InformesDeServicio.Messages.Publicadores.InProcess.Commands;

namespace InformesDeServicio.Messages.Publicadores.InProcess
{
    public interface IPublicadorInProcessSubscriber :
        ISubscribedTo<RegistrarPublicador>
    { }
}
