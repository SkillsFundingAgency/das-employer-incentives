using Newtonsoft.Json;
using SFA.DAS.HashingService;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Services.AccountApi
{
    public class AccountService : IAccountService
    {
        private readonly HttpClient _client;
        private readonly IHashingService _hashingService;

        public AccountService(
            HttpClient client,
            IHashingService hashingService)
        {
            _client = client;
            _hashingService = hashingService;
        }

        public async Task<PagedModel<AccountLegalEntity>> GetAccountLegalEntitiesByPage(int pageNumber, int pageSize = 1000)
        {
            var response = await _client.GetAsync($"api/accountlegalentities?pageNumber={pageNumber}&pageSize={pageSize}");

            response.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<PagedModel<AccountLegalEntity>>(await response.Content.ReadAsStringAsync());
        }
    }
}
