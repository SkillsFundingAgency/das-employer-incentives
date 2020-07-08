using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities.AcceptanceTests
{
    public static class HttpClientExtensions
    {
        public static async Task<(HttpStatusCode, T)> GetValueAsync<T>(this HttpClient client, string url)
        {
            using var response = await client.GetAsync(url);
            return await ProcessResponse<T>(response);
        }

        public static async Task<HttpResponseMessage> PostValueAsync<T>(this HttpClient client, string url, T data)
        {
            return await client.PostAsync(url, data.GetStringContent());
        }

        public static async Task<HttpResponseMessage> PutValueAsync<T>(this HttpClient client, string url, T data)
        {
            return await client.PutAsync(url, data.GetStringContent());
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
            => new StringContent(JsonConvert.SerializeObject(obj), Encoding.Default, "application/json");
    }
}
