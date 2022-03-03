using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Hooks;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests
{
    public class TestEventPublisher : IEventPublisher
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly IHook<object> _hook;
        private readonly CancellationToken _cancellationToken;

        public TestEventPublisher(IEventPublisher eventPublisher, IHook<object> hook, CancellationToken cancellationToken)
        {
            _eventPublisher = eventPublisher;
            _hook = hook;
            _cancellationToken = cancellationToken;
        }

        public async Task Publish<T>(T message) where T : class
        {
            if (_hook != null)
            {
                try
                {
                    if (_hook?.OnReceived != null)
                    {
                        _hook.OnReceived(message);
                    }

                    if(_cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    await _eventPublisher.Publish(message);

                    if (_hook?.OnProcessed != null)
                    {
                        _hook.OnProcessed(message);
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
                        suppressError = _hook.OnErrored(ex, message);
                    }
                    if (!suppressError)
                    {
                        throw;
                    }
                }
            }
            else
            {
                await _eventPublisher.Publish(message);
            }
        }

        public Task Publish<T>(Func<T> messageFactory) where T : class
        {
            return Publish(messageFactory.Invoke());
        }
    }
}
