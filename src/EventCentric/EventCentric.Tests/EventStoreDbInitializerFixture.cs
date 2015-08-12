using EventCentric.Database;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;

namespace EventCentric.Tests
{
    [TestClass]
    public class EventStoreDbInitializerFixture : IDisposable
    {
        protected string connectionString;
        protected bool dbWasCreated = false;

        public EventStoreDbInitializerFixture()
        {
            this.connectionString = ConfigurationManager.AppSettings["defaultConnection"];

            EventStoreDbInitializer.CreateDatabaseObjects(this.connectionString, true);
            this.dbWasCreated = true;
        }

        [TestMethod]
        public void Db_can_be_created()
        {
            Assert.IsTrue(this.dbWasCreated);
        }

        public void Dispose()
        {
            SqlClientLite.DropDatabase(this.connectionString);
        }
    }
}
