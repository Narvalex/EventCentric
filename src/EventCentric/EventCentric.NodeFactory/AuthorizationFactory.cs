using EventCentric.Authorization;
using EventCentric.Config;

namespace EventCentric
{
    public class AuthorizationFactory
    {
        public static void SetToken(IEventStoreConfig config)
        {
            Identity.Configure(config.Token);
        }
    }
}
