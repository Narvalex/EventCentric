using EventCentric.Utils;
using System;
using System.Configuration;

namespace EventCentric.Config
{
    public class EventStoreConfig : ConfigurationSection, IEventStoreConfig
    {
        private EventStoreConfig() { }

        public static IEventStoreConfig GetConfig()
        {
            var config = ConfigurationManager.GetSection("eventStoreConfig") as EventStoreConfig;
            Ensure.NotNull(config, "Event Store configuration");
            return config;
        }

        [ConfigurationProperty("connectionString", IsRequired = true)]
        public string ConnectionString => this["connectionString"] as string;

        [ConfigurationProperty("pushMaxCount", IsRequired = true)]
        public int PushMaxCount => Convert.ToInt32(this["pushMaxCount"]);

        [ConfigurationProperty("longPollingTimeout", IsRequired = true)]
        public double LongPollingTimeout => Convert.ToDouble(this["longPollingTimeout"]);

        [ConfigurationProperty("token", IsRequired = true)]
        public string Token => this["token"] as string;
    }
}
