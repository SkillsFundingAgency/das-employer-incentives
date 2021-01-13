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
        
        public Uri BaseAddress { get; private set; }
        private bool isDisposed;
        private HttpResponseMessage _response;

        public EmployerIncentiveApi(HttpClient client)
        {
            Client = client;
            BaseAddress = client.BaseAddress;
        }
        public HttpResponseMessage GetLastResponse()
        {
            return _response;
        }
        public async Task PostCommand<T>(string url, T command) where T : ICommand
        {
            var commandText = JsonConvert.SerializeObject(command, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            _response = await Client.PostAsJsonAsync(url, commandText);
        }

        public async Task Post<T>(string url, T data)
        {
            _response = await Client.PostValueAsync(url, data);            
        }

        public async Task Put<T>(string url, T data)
        {
            _response = await Client.PutValueAsync(url, data);            
        }

        public async Task Patch<T>(string url, T data)
        {
            _response = await Client.PatchValueAsync(url, data);
        }

        public async Task Delete(string url)
        {
            _response = await Client.DeleteAsync(url);
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
                _response?.Dispose();
                Client.Dispose();
            }

            isDisposed = true;
        }
    }
}
