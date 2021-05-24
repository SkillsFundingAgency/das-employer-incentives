using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators;
using SFA.DAS.EmployerIncentives.Functions.TestHelpers;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "OrchestrationStatus")]

    public class OrchestrationStatusSteps   
    {
        private readonly TestContext _testContext;

        public OrchestrationStatusSteps(TestContext testContext)
        {
            _testContext = testContext;
        }

        [Given(@"an orchestrator has been invoked")]
        public async Task GivenAnOrchestratorHasBeenInvoked()
        {
            await StartLearnerMatching();
        }
        
        [When(@"orchestrators check all status function is run")]
        public async Task WhenOrchestratorsCheckAllStatusFunctionIsRun()
        {
            await StartGetAllStatusOrchestrator();
        }
        
        [Then(@"the status results are shown")]
        public void ThenTheStatusResultsAreShown()
        {
           
        }

        private async Task StartGetAllStatusOrchestrator()
        {
            await _testContext.TestFunction.Start(
                new OrchestrationStarterInfo(
                    "GetAllStatus",
                    "GetAllStatus",
                    new Dictionary<string, object>
                    {
                        ["req"] = new DummyHttpRequest
                        {
                            Path = $"/api/orchestrators/GetAllStatus"
                        }
                    }
                ));
        }

        private async Task StartLearnerMatching()
        {
            await _testContext.TestFunction.Start(
                new OrchestrationStarterInfo(
                    "LearnerMatchingOrchestrator_Start",
                    "LearnerMatchingOrchestrator",
                    new Dictionary<string, object>
                    {
                        ["req"] = new DummyHttpRequest
                        {
                            Path = $"/api/orchestrators/LearnerMatchingOrchestrator"
                        }
                    }
                ));
        }
    }
}
