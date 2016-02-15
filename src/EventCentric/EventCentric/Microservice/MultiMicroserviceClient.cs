using EventCentric.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace EventCentric.Transport
{
    public class MultiMicroserviceClient<TEnum> : IMultiMicroserviceClient<TEnum> where TEnum : struct, IConvertible
    {
        private readonly string sharedToken;
        private readonly Dictionary<TEnum, string> microservices;

        public MultiMicroserviceClient(string sharedToken, params KeyValuePair<TEnum, string>[] nodes)
        {
            if (!typeof(TEnum).IsEnum)
                throw new InvalidOperationException("Type TEnum must be an enumeration");

            Ensure.Positive(nodes.Count(), $"{nameof(nodes)} count");
            Ensure.NotNull(sharedToken, nameof(sharedToken));

            this.microservices = new Dictionary<TEnum, string>(nodes.Count());
            this.microservices.AddRange(nodes);
            this.sharedToken = sharedToken;
        }

        public TResponse Send<TRequest, TResponse>(string url, TRequest payload)
        {
            HttpResponseMessage response;
            using (var client = this.HttpClientFactory())
            {
                response = client.PostAsJsonAsync(url, payload).Result;
            }
            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<TResponse>().Result;

            throw new HttpRequestException($"The attempt to make a request to {url} got a status code of {(int)response.StatusCode}.");
        }

        public TResponse Send<TRequest, TResponse>(TEnum node, string url, TRequest payload) => this.Send<TRequest, TResponse>(this.microservices[node] + url, payload);

        public IDictionary<TEnum, string> Nodes { get; }

        private HttpClient HttpClientFactory()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.sharedToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }
    }
}
