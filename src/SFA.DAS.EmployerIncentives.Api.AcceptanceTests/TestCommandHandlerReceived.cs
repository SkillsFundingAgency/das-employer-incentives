using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Hooks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests
{
    public class TestCommandHandlerReceived<T> : ICommandHandler<T> where T : ICommand
    {
        private readonly ICommandHandler<T> _handler;
        private readonly IHook<ICommand> _hook;

        public TestCommandHandlerReceived(
            ICommandHandler<T> handler,
            IHook<ICommand> hook)
        {
            _handler = handler;
            _hook = hook;
        }

        public async Task Handle(T command, CancellationToken cancellationToken = default)
        {
            if (_hook != null)
            {
                try
                {
                    if (_hook?.OnReceived != null)
                    {
                        _hook.OnReceived(command);
                    }
                    await _handler.Handle(command, cancellationToken);
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
                await _handler.Handle(command, cancellationToken);
            }
        }
    }
}
