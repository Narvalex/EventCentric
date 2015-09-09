using EventCentric.Processing;

namespace EasyTrade.Events.EmpresasQueue
{
    public interface IEmpresasQueueSubscriber
        : IEventHandler<NuevaEmpresaRegistrada>
    { }
}
