using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WebApiUtils.ApiAddresses;
using WebApiUtils.Entities;

namespace WebApiUtils
{
    public class HttpClient : IDisposable
    {
        public static HttpClientHandler CreateHandler() => new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        };

        private System.Net.Http.HttpClient client;

        public HttpClient()
        {
            var httpClientHelper = CreateHandler();
            client = new System.Net.Http.HttpClient(httpClientHelper);
            AuthClient.SetHttpAuth(client);
        }

        public HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage(client);
        }

        public void Dispose()
        {
            client.Dispose();
        }

        public class HttpRequestMessage
        {
            private System.Net.Http.HttpClient client;
            private System.Net.Http.HttpRequestMessage message;

            public HttpRequestMessage(System.Net.Http.HttpClient client)
            {
                this.client = client;
                message = new System.Net.Http.HttpRequestMessage();
            }

            public HttpRequestMessage SetMethod(HttpMethod method)
            {
                message.Method = method;
                return this;
            }

            public HttpRequestMessage SetMethodGet()
            {
                return SetMethod(HttpMethod.Get);
            }

            public HttpRequestMessage SetMethodPost()
            {
                return SetMethod(HttpMethod.Post);
            }

            public HttpRequestMessage SetUri(string uri)
            {
                message.RequestUri = new Uri(uri);
                return this;
            }

            public HttpRequestMessage SetContent<T>(T content)
            {
                message.Content = JsonContent.Create(content);
                return this;
            }

            public HttpResponseMessage Send()
            {
                return client.Send(message);
            }

            public async Task<HttpResponseMessage> SendAsync()
            {
                return await client.SendAsync(message);
            }
        }
    }

    public static class HttpClientExt
    {
        public static DResponse<T[]>? GetAllFrom<T>(this HttpClient client, BaseApiMethods api)
        {
            var result = client.CreateRequest()
                .SetMethodGet()
                .SetUri(api.GetAll)
                .SendAsync().Result;
            var content = result.Content;
            return content.ReadFromJsonAsync(typeof(DResponse<T[]>)).Result as DResponse<T[]>;
        }

        public static DResponse<T>? GetByIdFrom<T>(this HttpClient client, BaseApiMethods api, int id)
            where T : class
        {
            var result = client.CreateRequest()
                .SetMethodGet()
                .SetUri($"{api.GetById}?id={id}")
                .SendAsync().Result;
            var content = result.Content;
            return content.ReadFromJsonAsync(typeof(DResponse<T>)).Result as DResponse<T>;
        }

        public static DResponse<T>? AddFrom<T>(this HttpClient client, BaseApiMethods api, T item)
            where T : class
        {
            var result = client.CreateRequest()
                .SetMethodPost()
                .SetUri(api.Add)
                .SetContent(item)
                .SendAsync().Result;
            var content = result.Content;
            return content.ReadFromJsonAsync(typeof(DResponse<T>)).Result as DResponse<T>;
        }
    }
}
