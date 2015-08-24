using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;

namespace Clientes.ReadModel.Tests
{
    [TestClass]
    public class DbInitializerFixture
    {
        protected string connectionString;

        public DbInitializerFixture()
        {
            this.connectionString = ConfigurationManager.AppSettings["defaultConnection"];
        }

        [TestMethod]
        public void Can_create_read_model_db()
        {
        }
    }
}
