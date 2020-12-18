using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public abstract class ApiCommandControllerBase : ControllerBase
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

        protected Task SendCommandsAsync<TCommands>(TCommands commands) where TCommands : IEnumerable<ICommand>
        {
            return _commandDispatcher.SendMany(commands);
        }
    }
}