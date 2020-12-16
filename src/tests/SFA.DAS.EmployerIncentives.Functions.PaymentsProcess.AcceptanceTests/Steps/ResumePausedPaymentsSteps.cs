using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using FluentAssertions;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;
using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.Functions.TestHelpers;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "ResumePausedPayments")]
    public partial class ResumePausedPaymentsSteps
    {
        private readonly TestContext _testContext;

        private long _accountLegalEntityId;
        private PausePaymentsRequest _request;
        private ValidatePaymentsSteps.ValidatePaymentData _paymentData;

        public ResumePausedPaymentsSteps(TestContext testContext)
        {
            _testContext = testContext;
        }

        [Given(@"a non paused apprenticeship incentive exists")]
        public async Task GivenApprenticeshipIncentiveExists()
        {
            _paymentData = new ValidatePaymentsSteps.ValidatePaymentData(_testContext);
            await _paymentData.Create();
        }

        [Given(@"a paused apprenticeship incentive exists")]
        public async Task GivenAPausedApprenticeshipIncentiveExists()
        {
            _paymentData = new ValidatePaymentsSteps.ValidatePaymentData(_testContext);
            _paymentData.ApprenticeshipIncentiveModel.PausePayments = true;
            await _paymentData.Create();
        }

        [When(@"the resume payments request is sent")]
        public async Task WhenTheResumePausedPaymentsRequestIsSent()
        {
            _request = CreatePausePaymentsRequest(_paymentData.ApprenticeshipIncentiveModel.ULN, PausePaymentsAction.Resume);
            _accountLegalEntityId = _paymentData.AccountModel.AccountLegalEntityId;

            await SendRequestToEndpoint();
        }

        [When(@"an invalid request is sent")]
        public async Task WhenAnInvalidRequestIsSent()
        {
            _request = CreatePausePaymentsRequest(_paymentData.ApprenticeshipIncentiveModel.ULN, null);
            _accountLegalEntityId = _paymentData.AccountModel.AccountLegalEntityId;

            await SendRequestToEndpoint();
        }


        [Then(@"the requester is informed the apprenticeship incentive has resumed")]
        public void ThenTheRequesterIsInformedTheApprenticeshipIncentiveHasResumed()
        {
            _testContext.TestFunction.HttpObjectResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
            var content = _testContext.TestFunction.HttpObjectResult.Value;
            JsonConvert.SerializeObject(content).Should().Contain("Payments have been successfully Resumed");
        }

        [Then(@"the requester is informed the apprenticeship incentive is not paused")]
        public void ThenTheRequesterIsInformedTheApprenticeshipIncentiveIsAlreadyPaused()
        {
            _testContext.TestFunction.HttpObjectResult.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            var content = _testContext.TestFunction.HttpObjectResult.Value;
            JsonConvert.SerializeObject(content).Should().Contain("Payments are already paused");
        }

        [Then(@"the PausePayment status is set to false")]
        public void ThenThePausePaymentStatusIsSetToFalse()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var incentives = dbConnection.GetAll<ApprenticeshipIncentive>();

            incentives.Count().Should().Be(1);
            incentives.First().PausePayments.Should().BeFalse();
        }

        [Then(@"an Audit record has been added to record this resume request")]
        public void ThenAnAuditRecordHasBeenAddedToRecordThisResumePaymentsRequest()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var statusAudits = dbConnection.GetAll<IncentiveApplicationStatusAudit>();

            statusAudits.Count().Should().Be(1);
            var statusAudit = statusAudits.First();
            statusAudit.Process.Should().Be(IncentiveApplicationStatus.PaymentsResumed);
            statusAudit.IncentiveApplicationApprenticeshipId.Should().Be(_paymentData.ApprenticeshipIncentiveModel.IncentiveApplicationApprenticeshipId);
            statusAudit.ServiceRequestCreatedDate.Should().Be(_request.DateServiceRequestTaskCreated);
            statusAudit.ServiceRequestDecisionReference.Should().Be(_request.DecisionReferenceNumber);
            statusAudit.ServiceRequestTaskId.Should().Be(_request.ServiceRequestId);
        }

        [Then(@"the requester is informed the request is invalid")]
        public void ThenTheRequesterIsInformedTheRequestIsInvalid()
        {
            _testContext.TestFunction.HttpObjectResult.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            var content = _testContext.TestFunction.HttpObjectResult.Value;
            JsonConvert.SerializeObject(content).Should().Contain("Is not set");
        }

        public async Task SendRequestToEndpoint()
        {
            await _testContext.TestFunction.CallEndpoint(
                new EndpointInfo(
                    "PausePaymentsRequest",
                    new Dictionary<string, object>
                    {
                        ["req"] = new DummyHttpRequest(JsonConvert.SerializeObject(_request))
                        {
                            Path = $"/accountlegalentity/{_accountLegalEntityId}/payments",
                            Method = "patch"
                        },
                        ["accountLegalEntityId"] = _accountLegalEntityId
                    })
            );
        }

        private PausePaymentsRequest CreatePausePaymentsRequest(long uln, PausePaymentsAction? action)
        {
            return new PausePaymentsRequest
            {
                Action = action,
                ULN = uln,
                DateServiceRequestTaskCreated = DateTime.UtcNow,
                DecisionReferenceNumber = "DecisionReferenceNumber",
                ServiceRequestId = "ServiceRequestId"
            };
        }
    }
}