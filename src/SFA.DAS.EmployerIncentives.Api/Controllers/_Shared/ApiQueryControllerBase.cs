using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public abstract class ApiQueryControllerBase : ControllerBase
    {
        private readonly IQueryDispatcher _queryDispatcher;

        protected ApiQueryControllerBase(IQueryDispatcher queryDispatcher)
        {
            _queryDispatcher = queryDispatcher;
        }

        protected Task<TResult> QueryAsync<TQuery, TResult>(TQuery query) where TQuery : IQuery
        {
            return _queryDispatcher.Send<TQuery, TResult>(query);
        }
    }
}