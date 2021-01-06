using FluentAssertions;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Hooks;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Services
{
    public class TestHelper
    {
        private readonly TestContext _testContext;

        public TestHelper(TestContext testContext)
        {
            _testContext = testContext;
        }

        public async Task<WaitForResult> WaitFor<T>(
                   Func<Task> func,
                   bool assertOnTimeout = true,
                   bool assertOnError = false,
                   int timeoutInMs = 15000)
        {
            var waitForResult = new WaitForResult();

            var hook = _testContext.Hooks.SingleOrDefault(h => h is Hook<T>) as Hook<T>;

            hook.OnReceived += (message) => { waitForResult.SetHasStarted(); };
            hook.OnProcessed += (message) => { waitForResult.SetHasCompleted(); };
            hook.OnErrored += (ex, message) => { waitForResult.SetHasErrored(ex); };

            try
            {
                await func();
            }
            catch (Exception ex)
            {
                waitForResult.SetHasErrored(ex);
            }
            await WaitForHandlerCompletion(waitForResult, timeoutInMs);

            if (assertOnTimeout)
            {
                waitForResult.HasTimedOut.Should().Be(false, "handler should not have timed out");
            }

            if (assertOnError)
            {
                waitForResult.HasErrored.Should().Be(false, $"handler should not have failed with error '{waitForResult.LastException?.Message}'");
            }

            return waitForResult;
        }

        private static async Task WaitForHandlerCompletion(WaitForResult waitForResult, int timeoutInMs)
        {
            await using var timer = new Timer(TimedOutCallback, waitForResult, timeoutInMs, Timeout.Infinite);
            while (!waitForResult.HasCompleted && !waitForResult.HasTimedOut)
            {
                await Task.Delay(100);
            }
        }

        private static void TimedOutCallback(object state)
        {
            ((WaitForResult)state).SetHasTimedOut();
        }
    }
}
