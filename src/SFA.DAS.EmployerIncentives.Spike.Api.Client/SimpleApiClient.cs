using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Spike.Api.Client.Types;
using SFA.DAS.Http;

namespace SFA.DAS.EmployerIncentives.Spike.Api.Client
{
    public class SimpleApiClient : ISimpleApiClient
    {
        private readonly IRestHttpClient _client;

        public SimpleApiClient(IRestHttpClient client)
        {
            _client = client;
        }

        public Task<SimpleResponse> GetSimple()
        {
            return _client.Get<SimpleResponse>("simple");
        }

        public Task<SimpleResponse> PostSimple(SimpleRequest request)
        {
            return _client.PostAsJson<SimpleRequest, SimpleResponse>("simple", request);
        }
    }
}
