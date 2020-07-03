using FluentAssertions;
using Microsoft.Extensions.Hosting;
using SFA.DAS.EmployerIncentives.Data.UnitTests.TestHelpers;
using SFA.DAS.EmployerIncentives.Functions.LegalEntities.AcceptanceTests.Hooks;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities.AcceptanceTests
{
    public class TestContext
    {
        public DirectoryInfo TestDirectory { get; set; }

        public SqlDatabase SqlDatabase { get; set; }

        //public TestMessageBus TestMessageBus { get; set; }

        public IHost FunctionsHost { get; set; }
        public HttpClient ApiClient { get; set; }        

        public TestData TestData { get; set; }

        public CommandHandlerHooks CommandHandlerHooks { get; set; }

        public WaitForResult WaitForResult { get; set; }

        public TestContext()
        {
            TestDirectory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid().ToString()));
            if (!TestDirectory.Exists)
            {
                Directory.CreateDirectory(TestDirectory.FullName);
            }
            TestData = new TestData();
            CommandHandlerHooks = new CommandHandlerHooks();
        }

        public async Task WaitForHandler(
            Func<Task> func, 
            bool assertOnTimeout = true,
            bool assertOnError = false,
            int timeoutInMs = 15000)
        {
#if DEBUG
            timeoutInMs = 60000;
#endif
            WaitForResult = new WaitForResult();
          
            CommandHandlerHooks = new CommandHandlerHooks
            {
                OnHandlerStart = (command) => { WaitForResult.SetHasStarted(); },
                OnHandlerEnd = (command) => { WaitForResult.SetHasCompleted(); },                
                OnHandlerErrored = (ex, command) => { WaitForResult.SetHasErrored(ex); }
            };

            try
            {
                await func();
            }
            catch(Exception ex)
            {
                WaitForResult.SetHasErrored(ex);
            }
            await WaitForHandlerCompletion(WaitForResult, timeoutInMs);

            if(assertOnTimeout)
            {
                WaitForResult.HasTimedOut.Should().Be(false, "handler should not have timed out");
            }

            if (assertOnError)
            {
                WaitForResult.HasErrored.Should().Be(false, $"handler should not have errored with error '{WaitForResult.LastException?.Message}'");
            }
        }
        private bool hasTimedOut = false;
        private async Task WaitForHandlerCompletion(WaitForResult waitForResult, int timeoutInMs)
        {         
            using (Timer timer = new Timer(new TimerCallback(TimedOutCallback), null, timeoutInMs, Timeout.Infinite))
            {
                while (!waitForResult.HasCompleted && !waitForResult.HasTimedOut)
                {
                    await Task.Delay(100);
                }
            }
            if (hasTimedOut)
            {
                waitForResult.SetHasTimedOut();
            }
        }

        private void TimedOutCallback(object state)
        {
            hasTimedOut = true;
        }
    }
}
