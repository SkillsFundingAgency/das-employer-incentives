using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Queries;
using SFA.DAS.EmployerIncentives.Queries.Account;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountQueryController : ApiQueryControllerBase
    {
        public AccountQueryController(IQueryDispatcher queryDispatcher) : base(queryDispatcher)
        {
        }

        [HttpGet("/accounts/{accountId}/LegalEntities")]
        public Task<GetLegalEntitiesResponse> GetLegalEntities(long accountId)
        {
            var request = new GetLegalEntitiesRequest(accountId);

            return QueryAsync<GetLegalEntitiesRequest, GetLegalEntitiesResponse>(request);
        }
    }
}
