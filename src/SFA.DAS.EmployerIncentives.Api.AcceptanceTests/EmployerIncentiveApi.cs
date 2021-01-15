using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests
{
    public class EmployerIncentiveApi : IDisposable
    {
        public HttpClient Client { get; private set; }
        
        public Uri BaseAddress { get; private set; }
        private bool isDisposed;

        public EmployerIncentiveApi(HttpClient client)
        {
            Client = client;
            BaseAddress = client.BaseAddress;
        }
      
        public Task<HttpResponseMessage> PostCommand<T>(string url, T command) where T : ICommand
        {
            var commandText = JsonConvert.SerializeObject(command, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            return Client.PostAsJsonAsync(url, commandText);
        }

        public Task<HttpResponseMessage> Post<T>(string url, T data, CancellationToken cancellationToken = default)
        {
            return Client.PostValueAsync(url, data, cancellationToken);
        }

        public Task<HttpResponseMessage> Put<T>(string url, T data, CancellationToken cancellationToken = default)
        {
            return Client.PutValueAsync(url, data, cancellationToken);            
        }

        public Task<HttpResponseMessage> Patch<T>(string url, T data, CancellationToken cancellationToken = default)
        {
            return Client.PatchValueAsync(url, data, cancellationToken);
        }

        public Task<HttpResponseMessage> Delete(string url, CancellationToken cancellationToken = default)
        {
            return Client.DeleteAsync(url, cancellationToken);
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
