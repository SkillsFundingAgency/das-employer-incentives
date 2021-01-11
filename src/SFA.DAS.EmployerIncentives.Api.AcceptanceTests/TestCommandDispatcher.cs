using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Hooks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests
{
    public class TestCommandDispatcher : ICommandDispatcher
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IHook<ICommand> _hook;

        public TestCommandDispatcher(ICommandDispatcher commandDispatcher, IHook<ICommand> hook)
        {
            _commandDispatcher = commandDispatcher;
            _hook = hook;
        }

        public async Task Send<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand
        {
            if (_hook != null)
            {
                try
                {
                    if (_hook?.OnReceived != null)
                    {
                        _hook.OnReceived(command);
                    }
                    
                    await _commandDispatcher.Send(command);

                    if (_hook?.OnProcessed != null)
                    {
                        _hook.OnProcessed(command);
                    }
                }
                catch (Exception ex)
                {
                    bool suppressError = false;
                    if (_hook?.OnErrored != null)
                    {
                        suppressError = _hook.OnErrored(ex, command);
                    }
                    if (!suppressError)
                    {
                        throw;
                    }
                }
            }
            else
            {
                await _commandDispatcher.Send(command);
            }
        }

        public Task SendMany<TCommands>(TCommands commands, CancellationToken cancellationToken = default) where TCommands : IEnumerable<ICommand>
        {
            return _commandDispatcher.SendMany(commands, cancellationToken);
        }
    }
}
