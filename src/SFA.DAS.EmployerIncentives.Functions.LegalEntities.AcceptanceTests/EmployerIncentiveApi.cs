using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities.AcceptanceTests
{
    public class EmployerIncentiveApi : IDisposable
    {
        public HttpClient Client { get; private set; }
        public HttpResponseMessage Response { get; set; }
        private bool isDisposed;

        public EmployerIncentiveApi(HttpClient client)
        {
            Client = client;
        }

        public async Task Post<T>(string url, T data)
        {
            Response = await Client.PostValueAsync(url, data);
        }

        public async Task Put<T>(string url, T data)
        {
            Response = await Client.PutValueAsync(url, data);
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
