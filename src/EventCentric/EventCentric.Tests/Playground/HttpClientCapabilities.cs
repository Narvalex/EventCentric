using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;

namespace EventCentric.Tests.Playground.HttpClientCapabilities
{
    [TestClass]
    public class GIVEN_http_client
    {
        [TestMethod]
        public void WHEN_making_a_request_to_adressable_uri_THEN_returns_string()
        {
            var domain = "google";
            Action getStuff = () =>
            {
                using (var http = new HttpClient())
                {
                    var response = http.GetStringAsync($"http://www.{domain}.com").Result;

                    Assert.IsFalse(string.IsNullOrEmpty(response));
                }
            };

            getStuff.Invoke();
        }
    }
}
