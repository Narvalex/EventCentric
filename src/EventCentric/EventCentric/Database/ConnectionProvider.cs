using System.Configuration;

namespace EventCentric.Database
{
    public class ConnectionManager : ConfigurationSection, IConnectionProvider
    {
        private ConnectionManager() { }

        public static IConnectionProvider GetConnectionProvider()
        {
            return ConfigurationManager.GetSection("eventStoreConnection") as ConnectionManager;
        }

        [ConfigurationProperty("connectionString", IsRequired = true)]
        public string ConnectionString
        {
            get { return this["connectionString"] as string; }
        }
    }
}
