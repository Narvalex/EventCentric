using EventCentric.Config;

namespace EventCentric.MicroserviceFactory
{
    internal static class ConfigResolver
    {
        internal static IEventStoreConfig ResolveConfig(IEventStoreConfig providedConfig)
        {
            return providedConfig != null ? providedConfig : EventStoreConfig.GetConfig();
        }

        internal static IPollerConfig ResolveConfig(IPollerConfig providedConfig)
        {
            return providedConfig != null ? providedConfig : PollerConfig.GetConfig();
        }
    }
}
