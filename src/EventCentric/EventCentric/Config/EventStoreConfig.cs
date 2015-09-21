using System;
using System.Configuration;

namespace EventCentric.Config
{
    public class EventStoreConfig : ConfigurationSection, IEventStoreConfig
    {
        private EventStoreConfig() { }

        public static IEventStoreConfig GetConfig()
        {
            return ConfigurationManager.GetSection("eventStoreConfig") as EventStoreConfig;
        }

        [ConfigurationProperty("connectionString", IsRequired = true)]
        public string ConnectionString
        {
            get { return this["connectionString"] as string; }
        }

        [ConfigurationProperty("pushMaxCount", IsRequired = true)]
        public int PushMaxCount
        {
            get { return Convert.ToInt32(this["pushMaxCount"]); }
        }

        [ConfigurationProperty("longPollingTimeout", IsRequired = true)]
        public double LongPollingTimeout
        {
            get { return Convert.ToDouble(this["longPollingTimeout"]); }
        }

        [ConfigurationProperty("token", IsRequired = true)]
        public string Token
        {
            get { return this["token"] as string; }
        }
    }
}
