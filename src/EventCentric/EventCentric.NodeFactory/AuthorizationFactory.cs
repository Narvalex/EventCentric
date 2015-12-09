using EventCentric.Authorization;
using EventCentric.Config;

namespace EventCentric
{
    public static class AuthorizationFactory
    {
        public static void SetToken(IEventStoreConfig config)
            => AuthorizeEventSourcingAttribute.Configure(config.Token);
    }
}
