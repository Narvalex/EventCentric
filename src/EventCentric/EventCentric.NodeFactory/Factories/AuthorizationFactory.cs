using EventCentric.Authorization;
using EventCentric.Config;

namespace EventCentric.NodeFactory.Factories
{
    public class AuthorizationFactory
    {
        public static void SetToken(IEventStoreConfig config)
        {
            Identity.Configure(config.Token);
        }
    }
}
