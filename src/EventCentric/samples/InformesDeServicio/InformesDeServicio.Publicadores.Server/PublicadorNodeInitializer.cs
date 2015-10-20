using EventCentric;
using EventCentric.Messaging;
using EventCentric.Utils;
using Microsoft.Practices.Unity;

namespace InformesDeServicio.Publicadores.Server
{
    public class PublicadorNodeInitializer : NodeInitializer
    {
        public static void Initialize(IUnityContainer container)
        {
            Initialize(() =>
            {
                var node = ProessorNodeFactory<Publicador, PublicadorProcessor>
                        .CreateNodeWithApp<PublicadorApp>(container, false);

                var app = new PublicadorApp(container.Resolve<IEventBus>(), container.Resolve<IGuidProvider>(), container.Resolve<ITimeProvider>());

                // For asp.net controller dependency injection
                container.RegisterInstance<IPublicadorApp>(app);

                return node;
            });
        }
    }
}
