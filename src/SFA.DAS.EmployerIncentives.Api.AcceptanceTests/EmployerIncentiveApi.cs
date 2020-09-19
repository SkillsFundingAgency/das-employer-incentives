using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests
{
    public class EmployerIncentiveApi : IDisposable
    {
        public HttpClient Client { get; private set; }
        public HttpResponseMessage Response { get; set; }
        public Uri BaseAddress { get; private set; }
        private bool isDisposed;

        public EmployerIncentiveApi(HttpClient client)
        {
            Client = client;
            BaseAddress = client.BaseAddress;
        }

        public async Task PostCommand<T>(string url, T command) where T : ICommand
        {
            var commandText = JsonConvert.SerializeObject(command, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            var response = await Client.PostAsJsonAsync(url, commandText);

            Response = response;          
        }

        public async Task Post<T>(string url, T data)
        {
            Response = await Client.PostValueAsync(url, data);
        }

        public async Task Put<T>(string url, T data)
        {
            Response = await Client.PutValueAsync(url, data);
        }

        public async Task Patch<T>(string url, T data)
        {
            Response = await Client.PatchValueAsync(url, data);
        }

        public async Task Delete(string url)
        {
            Response = await Client.DeleteAsync(url);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;

            if (disposing)
            {
                Client.Dispose();
            }

            isDisposed = true;
        }
    }
}
