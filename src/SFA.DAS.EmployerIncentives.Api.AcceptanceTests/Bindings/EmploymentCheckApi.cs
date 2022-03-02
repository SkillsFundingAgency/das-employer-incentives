using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
{
    [Binding]
    [Scope(Tag = "employmentCheckApi")]
    public class EmploymentCheckApi
    {
        [BeforeScenario(Order = 5)]
        public void InitialiseEmploymentCheckApi(TestContext context)
        {
            context.EmploymentCheckApi = new TestEmploymentCheckApi();
        }

        [AfterScenario()]
        public void CleanUpEmploymentCheckApi(TestContext context)
        {
            context.EmploymentCheckApi.Dispose();
        }
    }
}
