using FluentAssertions;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators;
using SFA.DAS.EmployerIncentives.Functions.TestHelpers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using static SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Extensions.BlobContainerClientExtensions;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "MetricsReport")]
    public partial class MetricsReportSteps : StepsBase
    {
        private readonly TestContext _testContext;
        private ValidatePaymentsSteps.ValidatePaymentData _validatePaymentData;

        private const short CollectionPeriodYear = 2021;
        private const byte CollectionPeriod = 6;

        public MetricsReportSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
        }

        [Given(@"valid payments exist")]
        public async Task GivenValidPaymentsExist()
        {
            _validatePaymentData = new ValidatePaymentsSteps.ValidatePaymentData(_testContext);
            await _validatePaymentData.Create();
        }

        [When(@"the payment process is run")]
        public async Task WhenThePaymentProcessIsRun()
        {
            await _testContext.SetActiveCollectionCalendarPeriod(new CollectionPeriod() { Period = CollectionPeriod, Year = CollectionPeriodYear });

            await _testContext.TestFunction.Start(
               new OrchestrationStarterInfo(
                   "IncentivePaymentOrchestrator_HttpStart",
                   nameof(IncentivePaymentOrchestrator),
                   new Dictionary<string, object>
                   {
                       ["req"] = new DummyHttpRequest
                       {
                           Path = $"/api/orchestrators/IncentivePaymentOrchestrator"
                       }
                   },
                   expectedCustomStatus: "WaitingForPaymentApproval"
                   ));

            _testContext.TestFunction.LastResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

            var response = await _testContext.TestFunction.GetOrchestratorStartResponse();
            var status = await _testContext.TestFunction.GetStatus(response.Id);
            status.CustomStatus.ToObject<string>().Should().Be("WaitingForPaymentApproval");
        }

        [Then(@"the Metrics report is generated and sent")]
        public async Task ThenTheMetricsReportIsGeneratedAndSent()
        {
            _testContext.BlobClient.ItemCount().Should().Be(1);

            await foreach (var blobItem in _testContext.BlobClient.GetBlobsAsync())
            {
                blobItem.Name.Should().Be($"Metrics/LOCAL_ACCEPTANCE_TESTS Metrics R06_2021.xlsx");
                blobItem.Properties.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }            
        }

        [Then(@"a Slack message is posted to notify the Metrics report generation")]
        public void ThenASlackMessageIsPostedToNotityTheMetricsReportGeneration()
        {
            var command = TestContext.CommandsPublished.Single(c =>
                    c.IsPublished &&
                    c.Command is Commands.Types.PaymentProcess.SlackNotificationCommand).Command as Commands.Types.PaymentProcess.SlackNotificationCommand;

            command.SlackMessage.Should().NotBeNull();
            command.SlackMessage.Content.Should().Contain(nameof(MetricsReportGenerated));
        }
    }
}