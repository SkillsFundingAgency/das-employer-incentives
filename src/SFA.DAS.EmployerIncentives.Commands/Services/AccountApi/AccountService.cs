using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Services.AccountApi
{
    public class AccountService : IAccountService
    {
        private readonly HttpClient _client;

        public AccountService(HttpClient client)
        {
            _client = client;
        }

        public async Task<PagedModel<AccountLegalEntity>> GetAccountLegalEntitiesByPage(int pageNumber, int pageSize = 1000)
        {
            var response = await _client.GetAsync($"api/accountlegalentities?pageNumber={pageNumber}&pageSize={pageSize}");

            response.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<PagedModel<AccountLegalEntity>>(await response.Content.ReadAsStringAsync());
        }
    }
}
