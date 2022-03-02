using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
{
    [Binding]
    [Scope(Tag = "accountApi")]
    public class AccountApi
    {
        [BeforeScenario(Order = 4)]
        public void InitialiseAccountApi(TestContext context)
        {
            context.AccountApi = new TestAccountApi();
        }

        [AfterScenario()]
        public void CleanUpAccountApi(TestContext context)
        {
            context.AccountApi?.Dispose();
        }
    }
}
