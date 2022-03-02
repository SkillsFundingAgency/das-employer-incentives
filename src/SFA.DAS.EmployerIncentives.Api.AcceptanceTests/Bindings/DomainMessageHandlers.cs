using System;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
{
    [Binding]
    [Scope(Tag = "domainMessageHandlers")]
    public class DomainMessageHandlers
    {
        [BeforeScenario(Order = 6)]
        public async Task InitialiseFunctions(TestContext context)
        {
            context.DomainMessageHandlers = new TestDomainMessageHandlers(context);
            await context.DomainMessageHandlers.Start();
        }

        [AfterScenario()]
        public async Task CleanUp(TestContext context)
        {
            try
            {
                await context.DomainMessageHandlers.Stop();
            }
            catch (OperationCanceledException) { }
            context.DomainMessageHandlers?.Dispose();
        }
    }
}
