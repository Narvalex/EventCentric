using EventCentric.Respository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;

namespace EventCentric.Tests
{
    [TestClass]
    public class EventStoreDbInitializerFixture
    //: IDisposable
    {
        protected string connectionString;

        public EventStoreDbInitializerFixture()
        {
            this.connectionString = ConfigurationManager.AppSettings["defaultConnection"];
        }

        [TestMethod]
        public void Processor_node_db_can_be_created()
        {
            DbInitializer.CreateDatabaseObjects(this.connectionString, true);
        }

        [TestMethod]
        public void Client_node_db_can_be_created()
        {
            DbInitializer.CreateDatabaseObjects(this.connectionString, true, true);
        }

        //public void Dispose()
        //{
        //    SqlClientLite.DropDatabase(this.connectionString);
        //}
    }
}
