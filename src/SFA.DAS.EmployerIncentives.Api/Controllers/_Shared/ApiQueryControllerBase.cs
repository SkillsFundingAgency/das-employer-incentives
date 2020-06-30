using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Queries;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public abstract class ApiQueryControllerBase
    {
        private readonly IQueryDispatcher _queryDispatcher;

        protected ApiQueryControllerBase(IQueryDispatcher queryDispatcher)
        {
            _queryDispatcher = queryDispatcher;
        }

  
        protected async Task<TResult> QueryAsync<TQuery, TResult>(TQuery query) where TQuery : IQuery<TResult>
        {
            var response = await _queryDispatcher.Send<TQuery, TResult>(query);

            return response;
        }
    }
}