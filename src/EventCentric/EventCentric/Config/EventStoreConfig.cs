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

        [ConfigurationProperty("pollAttemptsMaxCount", IsRequired = true)]
        public int PollAttemptsMaxCount
        {
            get { return Convert.ToInt32(this["pollAttemptsMaxCount"]); }
        }
    }
}
