using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Application.Commands
{
    public class CommandHandlerWithLogging<T> : ICommandHandler<T> where T : ICommand
    {
        private readonly ICommandHandler<T> _handler;
        private readonly ILogger<T> _log;

        public CommandHandlerWithLogging(
            ICommandHandler<T> handler,         
            ILogger<T> log)
        {
            _handler = handler;
            _log = log;
        }

        public async Task Handle(T command)
        {   
            try
            {
                _log.LogInformation($"Start handle '{typeof(T)}' command");
                await _handler.Handle(command);
                _log.LogInformation($"End handle '{typeof(T)}' command");
            }
            catch(Exception ex)
            {
                _log.LogError(ex, $"Error handling '{typeof(T)}' command");
                throw;
            }
        }
    }
}
