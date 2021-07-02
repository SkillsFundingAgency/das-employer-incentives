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
        private readonly IHook<object> _hook;

        public TestScheduledCommandPublisher(IScheduledCommandPublisher messageSession, IHook<object> hook)
        {
            _messageSession = messageSession;
            _hook = hook;
        }

        public async Task Send<T>(T command, TimeSpan delay, CancellationToken cancellationToken = default(CancellationToken)) where T : class, ICommand
        {
            if (_hook != null)
            {
                try
                {
                    if (_hook?.OnReceived != null)
                    {
                        _hook.OnReceived(command);
                    }

                    await _messageSession.Send(command, delay, cancellationToken);

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
                await _messageSession.Send(command, delay, cancellationToken);
            }
        }
    }
}
