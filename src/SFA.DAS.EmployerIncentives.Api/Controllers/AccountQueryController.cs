using Microsoft.AspNetCore.Mvc;
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
        public AccountQueryController(IQueryDispatcher queryDispatcher) : base(queryDispatcher)
        {
        }

        [HttpGet("/accounts/{accountId}/LegalEntities")]
        public Task<GetLegalEntitiesResponse> GetLegalEntities(long accountId)
        {
            var request = new GetLegalEntitiesRequest(accountId);
            var response = QueryAsync<GetLegalEntitiesRequest, GetLegalEntitiesResponse>(request);
            
            ThrowIfNotFound(response.Result);
          
            return response;
        }

        private static void ThrowIfNotFound(GetLegalEntitiesResponse response)
        {
            if (response?.LegalEntities?.Count() > 0) return;

            throw new HttpResponseException(HttpStatusCode.NotFound);
        }
    }
}
