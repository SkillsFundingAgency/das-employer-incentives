using FluentAssertions;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators;
using SFA.DAS.EmployerIncentives.Functions.TestHelpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using static SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Extensions.BlobContainerClientExtensions;
using System;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "MetricsReport")]
    public partial class MetricsReportSteps : StepsBase
    {
        private readonly TestContext _testContext;
        private readonly string _storageDirectory;
        private ValidatePaymentsSteps.ValidatePaymentData _validatePaymentData;

        private const short CollectionPeriodYear = 2021;
        private const byte CollectionPeriod = 6;

        private static bool IncentivePaymentOrchestratorCompleted = false;
        private string _paymentRunStatus;

        public MetricsReportSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _storageDirectory = testContext.MessageBus.StorageDirectory.FullName;

            _testContext.PaymentsApi.MockServer
                   .Given(
                       Request
                           .Create()
                           .WithPath($"/payments/requests")
                           .WithHeader("Content-Type", "application/payments-data")
                           .WithParam("api-version", "2020-10-01")
                           .UsingPost()
                   )
                   .RespondWith(Response.Create()
                       .WithStatusCode(HttpStatusCode.Accepted)
                       .WithHeader("Content-Type", "application/json"));

        }

        [Given(@"valid payments exist")]
        public async Task GivenValidPaymentsExist()
        {
            _validatePaymentData = new ValidatePaymentsSteps.ValidatePaymentData(_testContext);
            await _validatePaymentData.Create();
        }

        [When(@"the payment process is run and approved")]
        public async Task WhenThePaymentProcessIsRunAndApproved()
        {
            await _testContext.SetActiveCollectionCalendarPeriod(new CollectionPeriod() { Period = CollectionPeriod, Year = CollectionPeriodYear });

            var paymentOrchestratorTask = IncentivePaymentOrchestratorTask();
            var emailReceivedTask = EmailReceivedAndAuthorisedTask();

            await Task.WhenAll(paymentOrchestratorTask, emailReceivedTask);
        }

        [When(@"the payment process is run but approvals are not received in time")]
        public async Task WhenThePaymentProcessIsRunAndNotApprovedInTime()
        {
            await _testContext.SetActiveCollectionCalendarPeriod(new CollectionPeriod() { Period = CollectionPeriod, Year = CollectionPeriodYear });

            await IncentivePaymentOrchestratorTask("PaymentsRejected");
        }
        private async Task IncentivePaymentOrchestratorTask(string expectedStatus = "PaymentsApproved")
        {
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
                    expectedCustomStatus: expectedStatus
                    ));

            IncentivePaymentOrchestratorCompleted = true;
            _paymentRunStatus = expectedStatus;
        }

        private async Task EmailReceivedAndAuthorisedTask()
        {
            while (IncentivePaymentOrchestratorCompleted == false)
            {
                var taskList = new List<Task>();

                var directoryInfo = new DirectoryInfo($"{_storageDirectory}\\SFA.DAS.Notifications.MessageHandlers\\.bodies\\");
                if (!directoryInfo.Exists)
                {
                    await Task.Delay(5000);
                    continue;
                }

                var recentFiles = directoryInfo.GetFiles().Where(x => x.CreationTimeUtc >= DateTime.UtcNow.AddMinutes(-2));

                if (recentFiles.Count() < _testContext.PaymentProcessSettings.MetricsReportEmailList.Count())
                {
                    await Task.Delay(1000);
                    continue;
                }

                foreach (var email in _testContext.PaymentProcessSettings.MetricsReportEmailList)
                {
                    foreach (var file in recentFiles)
                    {
                        var contents = File.ReadAllText(file.FullName, System.Text.Encoding.UTF8);
                        if (contents.Contains("metricsReportDownloadLink"))
                        {
                            var temp = contents[(contents.IndexOf("approvePayments/") + "approvePayments/".Length)..];
                            var approvalInstance = temp[1..temp.IndexOf("\"")];

                            taskList.Add(_testContext.TestFunction.Start(
                                new OrchestrationStarterInfo(
                                    "PaymentApproval_HttpStart",
                                    nameof(IncentivePaymentOrchestrator),
                                    new Dictionary<string, object>
                                    {
                                        ["req"] = new DummyHttpRequest
                                        {
                                            Path = $"/api/orchestrators/approvePayments/{approvalInstance}"
                                        },
                                        ["instanceId"] = approvalInstance
                                    })));
                        }                      
                    }                  
                }

                await Task.WhenAll(taskList);
            }

            return;
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
            var recentFiles = directoryInfo.GetFiles().Where(x => x.CreationTimeUtc >= DateTime.UtcNow.AddMinutes(-2));

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

        [Then(@"a Slack message is posted to notify the Metrics report generation")]
        public void ThenASlackMessageIsPostedToNotityTheMetricsReportGeneration()
        {
            var publishedCommand = TestContext.CommandsPublished.Single(c =>
                    c.IsPublished &&
                    c.Command is Commands.Types.PaymentProcess.SlackNotificationCommand).Command as Commands.Types.PaymentProcess.SlackNotificationCommand;

            publishedCommand.SlackMessage.Should().NotBeNull();
            publishedCommand.SlackMessage.Content.Should().Contain(nameof(MetricsReportGenerated));
        }

        [Then(@"the payment run is rejected")]
        public void ThenThePaymentRunIsrejected()
        {
            _paymentRunStatus.Should().Be("PaymentsRejected");
        }

        [Then(@"the payments can be sent to Business Central")]
        public Task ThenThePaymentCanBeSent()
        {
            var paymentRequestCount = _testContext.PaymentsApi.MockServer.LogEntries.Count(l => l.RequestMessage.AbsolutePath == "/payments/requests");
            paymentRequestCount.Should().Be(2);

            return Task.CompletedTask;
        }

        [Then(@"the payments are not sent to Business Central")]
        public Task ThenThePaymentsAreNotSent()
        {
            var paymentRequestCount = _testContext.PaymentsApi.MockServer.LogEntries.Count(l => l.RequestMessage.AbsolutePath == "/payments/requests");
            paymentRequestCount.Should().Be(0);

            return Task.CompletedTask;
        }        
    }
}