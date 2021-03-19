using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators;
using SFA.DAS.EmployerIncentives.Functions.TestHelpers;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "MonthEndInProgress")]
    public class MonthEndInProgressSteps
    {
        private TestContext _testContext;

        private const short CollectionPeriodYear = 2021;
        private const byte CollectionPeriod = 6;

        public MonthEndInProgressSteps(TestContext testContext)
        {
            _testContext = testContext;
        }

        [Given(@"the active collection period is not currently in progress")]
        public void GivenTheActiveCollectionPeriodIsNotCurrentlyInProgress()
        {
        }

        [When(@"the payment process is run")]
        public async Task WhenThePaymentProcessIsRun()
        {
            await _testContext.TestFunction.Start(
                new OrchestrationStarterInfo(
                    "IncentivePaymentOrchestrator_HttpStart",
                    nameof(IncentivePaymentOrchestrator),
                    new Dictionary<string, object>
                    {
                        ["req"] = new DummyHttpRequest
                        {
                            Path = $"/api/orchestrators/IncentivePaymentOrchestrator/{CollectionPeriodYear}/{CollectionPeriod}"
                        },
                        ["collectionPeriodYear"] = CollectionPeriodYear,
                        ["collectionPeriodNumber"] = CollectionPeriod
                    },
                    expectedCustomStatus: "WaitingForPaymentApproval"
                ));

            _testContext.TestFunction.LastResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

            var response = await _testContext.TestFunction.GetOrchestratorStartResponse();
            var status = await _testContext.TestFunction.GetStatus(response.Id);
            status.CustomStatus.ToObject<string>().Should().Be("WaitingForPaymentApproval");
        }

        [Then(@"the active collection period is set to in progress")]
        public async Task ThenTheValidationStepWillHaveAFailedValidationResult()
        {
            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var results = connection.GetAllAsync<Data.ApprenticeshipIncentives.Models.CollectionPeriod>().Result.Where(x => x.Active).ToList();
            results.Should().HaveCount(1);
            results.First().PeriodEndInProgress.Should().BeTrue();
        }
    }
}
