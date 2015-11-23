using EventCentric.Utils;
using System;
using System.Configuration;

namespace EventCentric.Config
{
    public class PollerConfig : ConfigurationSection, IPollerConfig
    {
        private PollerConfig() { }

        public static IPollerConfig GetConfig()
        {
            var config = ConfigurationManager.GetSection("pollerConfig") as PollerConfig;
            Ensure.NotNull(config, "Poller configuration");
            return config;
        }

        [ConfigurationProperty("bufferQueueMaxCount", IsRequired = true)]
        public int BufferQueueMaxCount => Convert.ToInt32(this["bufferQueueMaxCount"]);

        [ConfigurationProperty("eventsToFlushMaxCount", IsRequired = true)]
        public int EventsToFlushMaxCount => Convert.ToInt32(this["eventsToFlushMaxCount"]);

        [ConfigurationProperty("timeout", IsRequired = true)]
        public double Timeout => Convert.ToDouble(this["timeout"]);
    }
}
