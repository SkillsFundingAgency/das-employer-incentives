using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Commands;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ApiCommandControllerBase
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public ApiCommandControllerBase(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        protected async Task SendCommandAsync<TCommand>(TCommand command) where TCommand : ICommand
        {
            await _commandDispatcher.Send<TCommand>(command);
        }

        protected async Task<TResult> SendCommandAsync<TResult, TCommand>(TCommand command) where TCommand : ICommand
        {
            return await _commandDispatcher.Send<TResult, TCommand>(command);
        }
    }
}