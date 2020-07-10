using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Queries.Account;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountQueryController : ApiQueryControllerBase
    {
        private readonly ILogger<AccountQueryController> _logger;

        public AccountQueryController(IQueryDispatcher queryDispatcher,
            ILogger<AccountQueryController> logger) : base(queryDispatcher, logger)
        {
            _logger = logger;
        }

        [HttpGet("/accounts/{accountId}/LegalEntities")]
        public Task<GetLegalEntitiesResponse> GetLegalEntities(long accountId)
        {
            var request = new GetLegalEntitiesRequest(accountId);
            var response = QueryAsync<GetLegalEntitiesRequest, GetLegalEntitiesResponse>(request);

            ThrowIfNotFound(response.Result);

            return response;
        }

        private void ThrowIfNotFound(GetLegalEntitiesResponse response)
        {
            if (response?.LegalEntities?.Count() > 0) return;

            var ex = new HttpResponseException(HttpStatusCode.NotFound);
            _logger.LogError(ex, ErrorMessages.AccountNotFound);

            throw ex;
        }
    }
}
