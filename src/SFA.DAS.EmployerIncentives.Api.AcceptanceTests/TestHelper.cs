using FluentAssertions;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Hooks;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests
{
    public class TestHelper
    {
        private readonly TestContext _testContext;
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public TestHelper(TestContext testContext)
        {
            _testContext = testContext;
        }

        public async Task<WaitForResult> WaitFor(                   
                   Func<CancellationToken, Task> func,
                   Expression<Func<TestContext, bool>> predicate,
                   bool assertOnTimeout = true,
                   bool assertOnError = false,
                   int timeoutInMs = 60000)
        {
            var predicateDelegate = predicate.Compile();
            var token = _tokenSource.Token;
            _tokenSource.CancelAfter(timeoutInMs);

            var waitForResult = new WaitForResult();

            var hook = _testContext.Hooks.SingleOrDefault(h => h is Hook<ICommand>) as Hook<ICommand>;

            hook.OnReceived += (message) => { if (predicateDelegate.Invoke(_testContext)) { waitForResult.SetHasStarted(); } };
            hook.OnProcessed += (message) => { if (predicateDelegate.Invoke(_testContext)) { waitForResult.SetHasCompleted(); } };
            hook.OnPublished += (message) => { if( predicateDelegate.Invoke(_testContext)) { waitForResult.SetHasCompleted(); } };
            hook.OnErrored += (ex, message) => {
                if (!assertOnError)
                {
                    if (predicateDelegate.Invoke(_testContext))
                    {
                        waitForResult.SetHasErrored(ex);
                        waitForResult.SetHasCompleted();
                    }
                }
                return false;
            };

            try
            {
                try
                {
                    await Task.WhenAll(func(token), WaitForHandlerCompletion(waitForResult, timeoutInMs, token));
                }
                catch (Exception ex)
                {
                    waitForResult.SetHasErrored(ex);
                    _tokenSource.Cancel();
                }                

                if (assertOnTimeout)
                {
                    waitForResult.HasTimedOut.Should().Be(false, "handler should not have timed out");
                }

                if (assertOnError)
                {
                    waitForResult.HasErrored.Should().Be(false, $"handler should not have errored with error '{waitForResult.LastException?.Message}' and stack trace '{waitForResult.LastException?.StackTrace}'");
                }
            }
            finally
            {
                _tokenSource.Dispose();
            }

            return waitForResult;
        }
        
        public async Task<WaitForResult> WaitFor<T>(
                   Func<CancellationToken, Task> func,
                   bool assertOnTimeout = true,
                   bool assertOnError = false,
                   int timeoutInMs = 60000,
                   int numberOfOnProcessedEventsExpected = 1,
                   int numberOfOnPublishedEventsExpected = 0)
        {            
            var token = _tokenSource.Token;
            _tokenSource.CancelAfter(timeoutInMs);

            var waitForResult = new WaitForResult();
            var messagesProcessed = 0;
            var messagesPublished = 0;

            var hook = _testContext.Hooks.SingleOrDefault(h => h is Hook<T>) as Hook<T>;

            hook.OnReceived += (message) => { waitForResult.SetHasStarted(); };
            hook.OnProcessed += (message) => { messagesProcessed++; if (messagesProcessed >= numberOfOnProcessedEventsExpected && messagesPublished >= numberOfOnPublishedEventsExpected) { waitForResult.SetHasCompleted(); } };
            hook.OnPublished += (message) => { messagesPublished++; if (messagesProcessed >= numberOfOnProcessedEventsExpected && messagesPublished >= numberOfOnPublishedEventsExpected) { waitForResult.SetHasCompleted(); } };
            hook.OnErrored += (ex, message) => { waitForResult.SetHasErrored(ex); return false; };

            try
            {
                try
                {                    
                    await Task.WhenAll(func(token), WaitForHandlerCompletion(waitForResult, timeoutInMs, token));
                }
                catch (Exception ex)
                {
                    waitForResult.SetHasErrored(ex);
                    _tokenSource.Cancel();
                }

                if (assertOnTimeout)
                {
                    waitForResult.HasTimedOut.Should().Be(false, "handler should not have timed out");
                }

                if (assertOnError)
                {
                    waitForResult.HasErrored.Should().Be(false, $"handler should not have errored with error '{waitForResult.LastException?.Message}' and stack trace '{waitForResult.LastException?.StackTrace}'");
                }
            }
            finally
            {
                _tokenSource.Dispose();
            }
            
            return waitForResult;
        }

        private async Task WaitForHandlerCompletion(WaitForResult waitForResult, int timeoutInMs, CancellationToken cancellationToken)
        {
            using (Timer timer = new Timer(new TimerCallback(TimedOutCallback), waitForResult, timeoutInMs, Timeout.Infinite))
            {
                while (!waitForResult.HasCompleted && !waitForResult.HasTimedOut && !cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(200);
                }
            }
        }

        private void TimedOutCallback(object state)
        {
            ((WaitForResult)state).SetHasTimedOut();
            _tokenSource.Cancel();
        }
    }
}
