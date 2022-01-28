using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests
{
    public static class TestContextExtensions
    {
        public static Task<WaitForResult> WaitFor(
                   this TestContext context,
                   Func<CancellationToken, Task> func,
                   Expression<Func<TestContext, bool>> predicate,
                   bool assertOnTimeout = true,
                   bool assertOnError = true,
                   int timeoutInMs = 60000)
        {
            return new TestHelper(context)
                .WaitFor(
                func,
                predicate,
                assertOnTimeout: assertOnTimeout,
                assertOnError: assertOnError,
                timeoutInMs: timeoutInMs);
        }
            public static Task<WaitForResult> WaitFor<T>(
                   this TestContext context,
                   Func<CancellationToken, Task> func,
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
