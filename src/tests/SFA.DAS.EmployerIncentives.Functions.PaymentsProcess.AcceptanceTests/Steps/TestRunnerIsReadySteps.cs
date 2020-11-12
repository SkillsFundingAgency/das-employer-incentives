using NUnit.Framework;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "__TestRunnerIsReady__")]
    public class TestRunnerIsReadySteps
    {
        private readonly TestContext _testContext;

        public TestRunnerIsReadySteps(TestContext testContext)
        {
            _testContext = testContext;
        }

        [Given(@"tests are setup")]
        public void GivenTestsAreSetup()
        {
            Assert.NotNull(_testContext);
        }

        [When(@"a database instance")]
        public void GivenDatabase()
        {
            Assert.NotNull(_testContext.SqlDatabase);
        }

        [When(@"a learner match api")]
        public void GivenLearnerMatchApi()
        {
            Assert.NotNull(_testContext.LearnerMatchApi);
        }

        [When(@"a functions host")]
        public void GivenFunctionsHost()
        {
            Assert.NotNull(_testContext.PaymentsProcessFunctions);
        }

        [Then(@"test runner is ready")]
        public void ThenTestRunnerIsReady()
        {
            Assert.Pass();
        }
    }
}
