using EventCentric.Processing;

namespace EasyTrade.Events.EmpresasQueue
{
    public interface IEmpresasQueueSubscriber :
        ISubscribedTo<NuevaEmpresaRegistrada>,
        ISubscribedTo<EmpresaDesactivada>,
        ISubscribedTo<EmpresaReactivada>,
        ISubscribedTo<DatosDeEmpresaActualizados>
    { }
}
