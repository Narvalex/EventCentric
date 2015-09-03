using System;
using System.Configuration;

namespace EventCentric.Config
{
    public class PollerConfig : ConfigurationSection, IPollerConfig
    {
        private PollerConfig() { }

        public static IPollerConfig GetConfig()
        {
            return ConfigurationManager.GetSection("pollerConfig") as PollerConfig;
        }

        [ConfigurationProperty("bufferQueueMaxCount", IsRequired = true)]
        public int BufferQueueMaxCount
        {
            get { return Convert.ToInt32(this["bufferQueueMaxCount"]); }
        }

        [ConfigurationProperty("eventsToFlushMaxCount", IsRequired = true)]
        public int EventsToFlushMaxCount
        {
            get { return Convert.ToInt32(this["eventsToFlushMaxCount"]); }
        }

        [ConfigurationProperty("timeout", IsRequired = true)]
        public int Timeout
        {
            get { return Convert.ToInt32(this["timeout"]); }
        }
    }
}
