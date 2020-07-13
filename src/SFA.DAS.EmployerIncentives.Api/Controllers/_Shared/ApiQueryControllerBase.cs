using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public abstract class ApiQueryControllerBase : ControllerBase
    {
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly ILogger<AccountQueryController> _logger;

        protected ApiQueryControllerBase(IQueryDispatcher queryDispatcher, ILogger<AccountQueryController> logger)
        {
            _queryDispatcher = queryDispatcher;
            _logger = logger;
        }


        protected Task<TResult> QueryAsync<TQuery, TResult>(TQuery query) where TQuery : IQuery
        {
            try
            {
                return _queryDispatcher.Send<TQuery, TResult>(query);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _logger.LogError(e, $"Error occured while processing query {query.GetType().Name}.");
                throw;
            }
        }
    }
}