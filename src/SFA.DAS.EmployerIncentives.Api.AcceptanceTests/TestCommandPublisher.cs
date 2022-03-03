using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Hooks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests
{
    public class TestCommandPublisher : ICommandPublisher
    {
        private readonly ICommandPublisher _commandPublisher;
        private readonly IHook<ICommand> _hook;
        private CancellationToken _cancellationToken;

        public TestCommandPublisher(ICommandPublisher commandPublisher, IHook<ICommand> hook, CancellationToken cancellationToken)
        {
            _commandPublisher = commandPublisher;
            _hook = hook;
            _cancellationToken = cancellationToken;
        }

        public async Task Publish<T>(T command, CancellationToken cancellationToken = default) where T : class, ICommand
        {
            if (_hook != null)
            {
                try
                {
                    if (_hook?.OnReceived != null)
                    {
                        _hook.OnReceived(command);
                    }
                    
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    await _commandPublisher.Publish(command);

                    if (_hook?.OnPublished != null)
                    {
                        _hook.OnPublished(command);
                    }
                }
                catch (Exception ex)
                {
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

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
                await _commandPublisher.Publish(command);
            }
        }

        public Task Publish(object command, CancellationToken cancellationToken = default)
        {
            return _commandPublisher.Publish(command);
        }
    }
}
