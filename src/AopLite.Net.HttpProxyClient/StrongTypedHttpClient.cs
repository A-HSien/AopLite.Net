using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace AopLite.Net.HttpProxyClient
{
    public class StrongTypedHttpClient
    {
        private readonly HttpClient client;

        public StrongTypedHttpClient(HttpClient client)
        {
            this.client = client;
        }

        public async Task<T> GetAsync<T>(string url)
        {
            var heads = client.DefaultRequestHeaders;
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) throw new HttpRequestException(response.ToString());
            var result = await DeserializeResponseBody<T>(response);
            return result;
        }


        public async Task PostAsJsonAsync(string url, object body)
        {
            await Post(url, body);
        }
        public async Task<T> PostAsJsonAsync<T>(string url, object body)
        {
            var response = await Post(url, body);
            var result = await DeserializeResponseBody<T>(response);
            return result;
        }

        private async Task<HttpResponseMessage> Post(string url, object body)
        {
            var response = await client.PostAsJsonAsync(url, body);
            if (!response.IsSuccessStatusCode) throw new HttpRequestException(response.ToString());
            return response;
        }

        private static async Task<T> DeserializeResponseBody<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStreamAsync();
            var result = DeserializeJsonFromStream<T>(content);
            return result;
        }

        private static T DeserializeJsonFromStream<T>(Stream stream)
        {
            if (stream == null || stream.CanRead == false)
                return default(T);

            using (var streamReader = new StreamReader(stream))
            using (var jsonText = new JsonTextReader(streamReader))
            {
                var jsonSerializer = new JsonSerializer();
                var searchResult = jsonSerializer.Deserialize<T>(jsonText);
                return searchResult;
            }
        }
    }
}
