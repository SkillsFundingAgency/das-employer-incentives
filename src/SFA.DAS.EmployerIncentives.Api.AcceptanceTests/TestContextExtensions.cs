using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests
{
    public static class TestContextExtensions
    {
        public static Task<WaitForResult> WaitFor<T>(
                    this TestContext context,
                    Func<Task> func,
                    bool assertOnTimeout = true,
                    bool assertOnError = false,
                    int timeoutInMs = 15000)
        {
            return new TestHelper(context)
                .WaitFor<T>(func, assertOnTimeout: assertOnTimeout, assertOnError: assertOnError, timeoutInMs: timeoutInMs);
        }
    }
}
