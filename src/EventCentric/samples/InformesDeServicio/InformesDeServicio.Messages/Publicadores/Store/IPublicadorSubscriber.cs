using EventCentric.Processing;
using InformesDeServicio.Messages.Publicadores.Store.Events;

namespace InformesDeServicio.Messages.Publicadores.Store
{
    public interface IPublicadorSubscriber :
        ISubscribedTo<PublicadorRegistrado>
    { }
}
