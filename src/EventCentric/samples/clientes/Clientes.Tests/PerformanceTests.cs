using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;

namespace Clientes.Tests
{
    [TestClass]
    public class PerformanceTests
    {
        [TestMethod]
        public void Can_create_10_streams()
        {
            this.CreateStreams(10);
        }

        [TestMethod]
        public void Can_create_50_streams()
        {
            this.CreateStreams(50);
        }

        [TestMethod]
        public void Can_create_100_streams()
        {
            this.CreateStreams(100);
        }

        private void CreateStreams(int quantity)
        {
            using (var http = new HttpClient())
            {
                for (int i = 0; i < quantity; i++)
                {
                    var response = http.GetAsync("http://localhost:61065/solicitudnuevoclienterecibida/cliente" + i.ToString()).Result;
                    Assert.IsTrue(response.IsSuccessStatusCode);
                }
            }
        }
    }
}