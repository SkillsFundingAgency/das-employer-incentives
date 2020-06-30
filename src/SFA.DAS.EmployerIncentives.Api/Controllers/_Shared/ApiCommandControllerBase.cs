using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Commands;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public abstract class ApiCommandControllerBase
    {
        private readonly ICommandDispatcher _commandDispatcher;

        protected ApiCommandControllerBase(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        protected async Task SendCommandAsync<TCommand>(TCommand command) where TCommand : ICommand
        {
            await _commandDispatcher.SendAsync(command);
        }
    }
}