using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests
{
    public static class HttpClientExtensions
    {
        public static async Task<(HttpStatusCode, T)> GetValueAsync<T>(this HttpClient client, string url, CancellationToken cancellationToken = default)
        {
            using var response = await client.GetAsync(url, cancellationToken);
            return await ProcessResponse<T>(response);
        }

        public static async Task<HttpResponseMessage> PostValueAsync<T>(this HttpClient client, string url, T data, CancellationToken cancellationToken = default)
        {
            return await client.PostAsync(url, data.GetStringContent(), cancellationToken);
        }

        public static async Task<HttpResponseMessage> PutValueAsync<T>(this HttpClient client, string url, T data, CancellationToken cancellationToken = default)
        {
            return await client.PutAsync(url, data.GetStringContent(), cancellationToken);
        }

        public static async Task<HttpResponseMessage> PatchValueAsync<T>(this HttpClient client, string url, T data, CancellationToken cancellationToken = default)
        {
            return await client.PatchAsync(url, data.GetStringContent(), cancellationToken);
        }

        private static async Task<(HttpStatusCode, T)> ProcessResponse<T>(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NoContent)
                return (response.StatusCode, default);

            var content = await response.Content.ReadAsStringAsync();
            var responseValue = JsonConvert.DeserializeObject<T>(content);

            return (response.StatusCode, responseValue);
        }

        public static StringContent GetStringContent(this object obj)
            => new StringContent(JsonConvert.SerializeObject(obj), System.Text.Encoding.Default, "application/json");
    }
}
