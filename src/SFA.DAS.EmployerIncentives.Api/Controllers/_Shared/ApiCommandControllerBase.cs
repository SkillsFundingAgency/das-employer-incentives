using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System.Threading.Tasks;

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

        protected Task SendCommandAsync<TCommand>(TCommand command) where TCommand : ICommand
        {
            return _commandDispatcher.Send(command);
        }
    }
}