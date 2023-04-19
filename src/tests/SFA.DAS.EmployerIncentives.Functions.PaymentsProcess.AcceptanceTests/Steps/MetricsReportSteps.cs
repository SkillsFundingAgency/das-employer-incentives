using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators;
using SFA.DAS.EmployerIncentives.Functions.TestHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using static SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Extensions.BlobContainerClientExtensions;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "MetricsReport")]
    public partial class MetricsReportSteps
    {
        private readonly TestContext _testContext;
        private readonly string _storageDirectory;
        private ValidatePaymentsSteps.ValidatePaymentData _validatePaymentData;

        private const short CollectionPeriodYear = 2021;
        private const byte CollectionPeriod = 6;

        public MetricsReportSteps(TestContext testContext)
        {
            _testContext = testContext;
            _storageDirectory = testContext.MessageBus.StorageDirectory.FullName;
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

        [Then("the Metrics report emails are sent")]
        public void ThenTheMetricsReportEmailsAreSent()
        {
            var directoryInfo = new DirectoryInfo($"{_storageDirectory}\\SFA.DAS.Notifications.MessageHandlers\\.bodies\\");
            var recentFiles = directoryInfo.GetFiles().Where(x => x.CreationTimeUtc >= DateTime.Now.AddMinutes(-2));

            recentFiles.Should().HaveCount(_testContext.PaymentProcessSettings.MetricsReportEmailList.Count);

            foreach (var email in _testContext.PaymentProcessSettings.MetricsReportEmailList)
            {
                bool isOk = false;
                foreach (var file in recentFiles)
                {
                    var contents = File.ReadAllText(file.FullName, System.Text.Encoding.UTF8);
                    if (contents.Contains(email) &&
                        contents.Contains("\"periodName\":\"R06\"") &&
                        contents.Contains("\"academicYear\":\"2021\"") &&
                        contents.Contains("\"metricsReportDownloadLink\":") &&
                        contents.Contains(_testContext.MetricsReportEmailGuid.ToString()))
                    {
                        isOk = true;
                        break;
                    }
                }

                if (!isOk)
                {
                    Assert.Fail($"No NServiceBus message found with the metrics report sent to {email}");
                }
            }
        }
    }
}