using EventCentric.Config;
using System;

namespace EventCentric.MicroserviceFactory
{
    internal static class ConfigResolver
    {
        internal static IEventStoreConfig ResolveConfig(IEventStoreConfig providedConfig)
        {
            try
            {
                return providedConfig != null ? providedConfig : EventStoreConfig.GetConfig();
            }
            catch (ArgumentNullException)
            {
                return new HardcodedEventStoreConfig();
            }

        }

        internal static IPollerConfig ResolveConfig(IPollerConfig providedConfig)
        {
            try
            {
                return providedConfig != null ? providedConfig : PollerConfig.GetConfig();
            }
            catch (ArgumentNullException)
            {
                return new HardcodedPollerConfig();
            }
        }
    }
}
