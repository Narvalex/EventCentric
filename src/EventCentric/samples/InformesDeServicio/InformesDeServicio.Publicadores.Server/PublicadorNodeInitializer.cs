using EventCentric;
using Microsoft.Practices.Unity;

namespace InformesDeServicio.Publicadores.Server
{
    public class PublicadorNodeInitializer : NodeInitializer
    {
        public static void Initialize(IUnityContainer container)
        {
            Initialize(() =>
            {
                return ProessorNodeFactory<Publicador, PublicadorProcessor>
                        .CreateNodeWithApp<PublicadorApp>(container, false);
            });
        }
    }
}
