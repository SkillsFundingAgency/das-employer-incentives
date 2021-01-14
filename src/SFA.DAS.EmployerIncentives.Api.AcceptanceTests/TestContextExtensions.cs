using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests
{
    public static class TestContextExtensions
    {
        public static Task<WaitForResult> WaitFor<T>(
                    this TestContext context,
                    Func<Task> func,
                    bool assertOnTimeout = true,
                    bool assertOnError = true,
                    int timeoutInMs = 90000,
                    int numberOfOnProcessedEventsExpected = 1,
                    int numberOfOnPublishedEventsExpected = 0)
        {
            return new TestHelper(context)
                .WaitFor<T>(
                func, 
                assertOnTimeout: assertOnTimeout, 
                assertOnError: assertOnError, 
                timeoutInMs: timeoutInMs, 
                numberOfOnProcessedEventsExpected: numberOfOnProcessedEventsExpected,
                numberOfOnPublishedEventsExpected: numberOfOnPublishedEventsExpected);
        }
    }
}
