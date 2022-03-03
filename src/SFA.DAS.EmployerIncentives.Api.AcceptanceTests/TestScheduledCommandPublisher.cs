using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Hooks;
using ICommand = SFA.DAS.EmployerIncentives.Abstractions.Commands.ICommand;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests
{
    public class TestScheduledCommandPublisher : IScheduledCommandPublisher
    {
        private readonly IScheduledCommandPublisher _messageSession;
        private readonly IHook<ICommand> _hook;
        private readonly CancellationToken _cancellationToken;

        public TestScheduledCommandPublisher(IScheduledCommandPublisher messageSession, IHook<ICommand> hook, CancellationToken cancellationToken)
        {
            _messageSession = messageSession;
            _hook = hook;
            _cancellationToken = cancellationToken;
        }

        public async Task Send<T>(T command, TimeSpan delay, CancellationToken cancellationToken = default(CancellationToken)) where T : ICommand
        {
            if (_hook != null)
            {
                try
                {
                    if (_hook?.OnReceived != null)
                    {
                        _hook.OnReceived(command);
                    }
                    
                    if(_cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    await _messageSession.Send(command, delay, cancellationToken);

                    if (_hook?.OnDelayed != null)
                    {
                        _hook.OnDelayed(command);
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
                await _messageSession.Send(command, delay, cancellationToken);
            }
        }
    }
}
