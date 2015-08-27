using EventCentric.Database;
using EventCentric.Respository;
using System;
using System.Configuration;

namespace EventCentric.Tests.Helpers
{
    public class GIVEN_db : IDisposable
    {
        protected string connectionString;

        public GIVEN_db()
        {
            this.connectionString = ConfigurationManager.AppSettings["defaultConnection"];
            DbInitializer.CreateDatabaseObjects(connectionString, true);
        }

        public void Dispose()
        {
            SqlClientLite.DropDatabase(this.connectionString);
        }
    }
}
