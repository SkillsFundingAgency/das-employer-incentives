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
        private readonly IQueryProvider _queryProvider;

        protected ApiQueryControllerBase(IQueryProvider queryProvider)
        {
            _queryProvider = queryProvider;
        }

  
        protected async Task<TResponse> QueryAsync<TResponse, TQuery>(TQuery query) where TQuery : IQuery
        {
            return await _queryProvider.Execute<TResponse, TQuery>(query);
        }
    }
}