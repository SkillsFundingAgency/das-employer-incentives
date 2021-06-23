using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "WithdrawalByCompliance")]
    public class WithdrawalByComplianceSteps : StepsBase
    {
        private readonly TestContext _testContext;
        private readonly Fixture _fixture;
        private readonly string _connectionString;
        private readonly IncentiveApplication _application;
        private readonly IncentiveApplicationApprenticeship _apprenticeship;
        private WithdrawApplicationRequest _withdrawApplicationRequest;
        private readonly IncentiveApplicationApprenticeship _apprenticeship2;

        private readonly ApprenticeshipIncentive _apprenticeshipIncentive;
        private readonly Payment _payment;
        private readonly PendingPayment _pendingPayment;
        private readonly PendingPayment _paidPendingPayment;
        private readonly PendingPaymentValidationResult _pendingPaymentValidationResult;
        private HttpResponseMessage _response;
        private bool _isMultipleApplications;

        public WithdrawalByComplianceSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();
            _connectionString = _testContext.SqlDatabase.DatabaseInfo.ConnectionString;

            _application = _fixture.Create<IncentiveApplication>();
            _apprenticeship = _fixture
                .Build<IncentiveApplicationApprenticeship>()
                .With(a => a.IncentiveApplicationId, _application.Id)
                .With(a => a.WithdrawnByCompliance, false)
                .Create();

            _apprenticeship2 = _fixture
                .Build<IncentiveApplicationApprenticeship>()
                .With(a => a.IncentiveApplicationId, _application.Id)
                .With(a => a.ULN, _apprenticeship.ULN)
                .With(a => a.WithdrawnByCompliance, false)
                .Create();

            _apprenticeshipIncentive = _fixture
                .Build<ApprenticeshipIncentive>()
                .With(i => i.IncentiveApplicationApprenticeshipId, _apprenticeship.Id)
                .With(i => i.AccountLegalEntityId, _application.AccountLegalEntityId)
                .With(i => i.AccountId, _application.AccountId)
                .With(i => i.ULN, _apprenticeship.ULN)
                .With(i => i.Status, IncentiveStatus.Active)
                .Create();

            _pendingPayment = _fixture
                .Build<PendingPayment>()
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(i => i.AccountId, _apprenticeshipIncentive.AccountId)
                .With(i => i.AccountLegalEntityId, _apprenticeshipIncentive.AccountLegalEntityId)
                .With(p => p.PaymentMadeDate, (DateTime?)null)
                .Create();

            _paidPendingPayment = _fixture
                .Build<PendingPayment>()
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(i => i.AccountId, _apprenticeshipIncentive.AccountId)
                .With(i => i.AccountLegalEntityId, _apprenticeshipIncentive.AccountLegalEntityId)
                .Create();

            _payment = _fixture
                .Build<Payment>()
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(i => i.AccountId, _apprenticeshipIncentive.AccountId)
                .With(i => i.AccountLegalEntityId, _apprenticeshipIncentive.AccountLegalEntityId)
                .With(p => p.PendingPaymentId, _paidPendingPayment.Id)
                .Create();

            _pendingPaymentValidationResult = _fixture
                .Build<PendingPaymentValidationResult>()
                .With(p => p.PendingPaymentId, _pendingPayment.Id)
                .With(p => p.Step, "Invalid")
                .With(p => p.PeriodNumber, 1)
                .With(p => p.PaymentYear, 2021)
                .Create();
        }

        [Given(@"an incentive application has been made without being submitted")]
        public async Task GivenAnIncentiveApplicationHasBeenMadeWithoutBeingSubmitted()
        {
            using var dbConnection = new SqlConnection(_connectionString);
            await dbConnection.InsertAsync(_application);
            await dbConnection.InsertAsync(_apprenticeship);
        }

        [Given(@"multiple incentive applications have been made for the same ULN without being submitted")]
        public async Task GivenMultiplwIncentiveApplicationsHaveBeenMadeWithoutBeingSubmitted()
        {
            using var dbConnection = new SqlConnection(_connectionString);
            await dbConnection.InsertAsync(_application);
            await dbConnection.InsertAsync(_apprenticeship);
            await dbConnection.InsertAsync(_apprenticeship2);
            _isMultipleApplications = true;
        }

        [Given(@"an apprenticeship incentive with pending payments exists as a result of an incentive application")]
        public async Task GivenAnApprenticeshipIncentiveWithPendingPaymentsExistsForAnApplication()
        {
            using var dbConnection = new SqlConnection(_connectionString);
            await dbConnection.InsertAsync(_application);
            await dbConnection.InsertAsync(_apprenticeship);
            await dbConnection.InsertAsync(_apprenticeshipIncentive);
            await dbConnection.InsertAsync(_pendingPayment);
            await dbConnection.InsertAsync(_pendingPaymentValidationResult);
        }

        [Given(@"an apprenticeship incentive with paid payments exists as a result of an incentive application")]
        public async Task GivenAnApprenticeshipIncentiveWithPaidPaymentsExistsForAnApplication()
        {
            using var dbConnection = new SqlConnection(_connectionString);
            await dbConnection.InsertAsync(_application);
            await dbConnection.InsertAsync(_apprenticeship);
            await dbConnection.InsertAsync(_apprenticeshipIncentive);
            await dbConnection.InsertAsync(_pendingPayment);
            await dbConnection.InsertAsync(_pendingPaymentValidationResult);
            await dbConnection.InsertAsync(_paidPendingPayment);
            await dbConnection.InsertAsync(_payment);
        }

        [When(@"the apprenticeship application is withdrawn from the scheme")]
        public async Task WhenTheApprenticeshipApplicationIsWithdrawnFromTheScheme()
        {
            _withdrawApplicationRequest = _fixture
                .Build<WithdrawApplicationRequest>()
                .With(r => r.WithdrawalType, WithdrawalType.Compliance)
                .With(r => r.AccountLegalEntityId, _application.AccountLegalEntityId)
                .With(r => r.ULN, _apprenticeship.ULN)
                .Create();

            var url = $"withdrawals";

            await _testContext.WaitFor(
                async (cancellationToken) =>
                {
                    _response = await EmployerIncentiveApi.Post(url, _withdrawApplicationRequest, cancellationToken);
                },
                (context) => HasExpectedEvents(context)
                );
        }

        private bool HasExpectedEvents(TestContext testContext)
        {
            var processedEvents = testContext.CommandsPublished.Count(c => c.IsProcessed && c.Command is WithdrawCommand);
            if(_isMultipleApplications)
            {
                return processedEvents == 2;
            }
            return processedEvents == 1;
        }

        [Then(@"the incentive application status is updated to indicate the Compliance withdrawal")]
        public async Task ThenTheIncentiveApplicationStatusIsUpdatedToIndicateTheComplianceWithdrawal()
        {
            _response.StatusCode.Should().Be(HttpStatusCode.Accepted);

            await using var dbConnection = new SqlConnection(_connectionString);
            var apprenticeships = await dbConnection.GetAllAsync<IncentiveApplicationApprenticeship>();
            apprenticeships.Should().HaveCount(1);
            apprenticeships.Single(a => a.Id == _apprenticeship.Id).WithdrawnByCompliance.Should().BeTrue();

            var incentiveApplicationAudits = await dbConnection.GetAllAsync<IncentiveApplicationStatusAudit>();
            incentiveApplicationAudits.Should().HaveCount(1);
            var auditRecord = incentiveApplicationAudits.Single(a => a.IncentiveApplicationApprenticeshipId == _apprenticeship.Id);
            auditRecord.Process.Should().Be(IncentiveApplicationStatus.ComplianceWithdrawn);
            auditRecord.ServiceRequestTaskId.Should().Be(_withdrawApplicationRequest.ServiceRequest.TaskId);
            auditRecord.ServiceRequestDecisionReference.Should().Be(_withdrawApplicationRequest.ServiceRequest.DecisionReference);
            auditRecord.ServiceRequestCreatedDate.Should().Be(_withdrawApplicationRequest.ServiceRequest.TaskCreatedDate.Value);

            var publishedCommand = _testContext
                .CommandsPublished
                .Single(c => c.IsPublished &&
                c.Command is WithdrawCommand).Command as WithdrawCommand;

            publishedCommand.AccountId.Should().Be(_application.AccountId);
            publishedCommand.IncentiveApplicationApprenticeshipId.Should().Be(_apprenticeship.Id);
        }

        [Then(@"each incentive application status is updated to indicate the Compliance withdrawal")]
        public async Task ThenEachIncentiveApplicationStatusIsUpdatedToIndicateTheComplianceWithdrawal()
        {
            _response.StatusCode.Should().Be(HttpStatusCode.Accepted);

            await using var dbConnection = new SqlConnection(_connectionString);
            var apprenticeships = await dbConnection.GetAllAsync<IncentiveApplicationApprenticeship>();
            apprenticeships.Should().HaveCount(2);
            apprenticeships.Single(a => a.Id == _apprenticeship.Id).WithdrawnByCompliance.Should().BeTrue();
            apprenticeships.Single(a => a.Id == _apprenticeship2.Id).WithdrawnByCompliance.Should().BeTrue();

            var incentiveApplicationAudits = await dbConnection.GetAllAsync<IncentiveApplicationStatusAudit>();
            incentiveApplicationAudits.Should().HaveCount(2);

            incentiveApplicationAudits.Single(a => a.IncentiveApplicationApprenticeshipId == _apprenticeship.Id).Process.Should().Be(IncentiveApplicationStatus.ComplianceWithdrawn);
            incentiveApplicationAudits.Single(a => a.IncentiveApplicationApprenticeshipId == _apprenticeship2.Id).Process.Should().Be(IncentiveApplicationStatus.ComplianceWithdrawn);

            _testContext
                .CommandsPublished
                .Count(c => c.IsPublished &&
                c.Command is WithdrawCommand)
                .Should().Be(2);
        }
        [Then(@"the apprenticeship incentive is marked as withdrawn and it's pending payments are removed from the system")]
        public async Task ThenTheIncentiveIsWithdrawnAndPendingPaymentsAreRemovedFromTheSystem()
        {
            await ThenTheIncentiveApplicationStatusIsUpdatedToIndicateTheComplianceWithdrawal();

            await using var dbConnection = new SqlConnection(_connectionString);
            var incentives = await dbConnection.GetAllAsync<ApprenticeshipIncentive>();
            var pendingPaymentValidationResults = await dbConnection.GetAllAsync<PendingPaymentValidationResult>();
            var pendingPayments = await dbConnection.GetAllAsync<PendingPayment>();

            incentives.Should().HaveCount(1); 
            var incentive = incentives.FirstOrDefault();
            incentive.Status.Should().Be(IncentiveStatus.Withdrawn);
            pendingPaymentValidationResults.Should().HaveCount(0);
            pendingPayments.Should().HaveCount(0);
        }

        [Then(@"clawbacks are created for the apprenticeship incentive payments and it's pending payments are archived")]
        public async Task ThenClawbacksAreCreatedForTheIncentiveAndItsPendingPaymentsAreArchived()
        {
            await ThenTheIncentiveApplicationStatusIsUpdatedToIndicateTheComplianceWithdrawal();

            await using var dbConnection = new SqlConnection(_connectionString);
            var incentives = await dbConnection.GetAllAsync<ApprenticeshipIncentive>();
            var pendingPaymentValidationResults = await dbConnection.GetAllAsync<PendingPaymentValidationResult>();
            var pendingPayments = await dbConnection.GetAllAsync<PendingPayment>();
            var payments = await dbConnection.GetAllAsync<Payment>();
            var clawbackPayments = await dbConnection.GetAllAsync<ClawbackPayment>();
            var archivedPendingPayments = await dbConnection.GetAllAsync<Data.ApprenticeshipIncentives.Models.Archive.PendingPayment>();
            var archivedPendingPaymentValidationResults = await dbConnection.GetAllAsync<Data.ApprenticeshipIncentives.Models.Archive.PendingPaymentValidationResult>();

            pendingPaymentValidationResults.Should().HaveCount(0);
            incentives.Should().HaveCount(1);
            payments.Should().HaveCount(1); // the paid payment
            pendingPayments.Should().HaveCount(1); // for the paid payment
            clawbackPayments.Should().HaveCount(1); // the paid amount
            archivedPendingPayments.Should().HaveCount(1); // the unpaid payment
            archivedPendingPaymentValidationResults.Should().HaveCount(1); // the unpaid payment

            var incentive = incentives.Single();
            incentive.Status.Should().Be(IncentiveStatus.Withdrawn);
            incentive.PausePayments.Should().BeFalse();

            payments.Single().Id.Should().Be(_payment.Id);
            pendingPayments.Single().Id.Should().Be(_paidPendingPayment.Id);

            var clawbackPayment = clawbackPayments.Single();
            clawbackPayment.AccountId.Should().Be(_apprenticeshipIncentive.AccountId);
            clawbackPayment.AccountLegalEntityId.Should().Be(_apprenticeshipIncentive.AccountLegalEntityId);
            clawbackPayment.Amount.Should().Be(_paidPendingPayment.Amount * -1);
            clawbackPayment.ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            clawbackPayment.CollectionPeriod.Should().Be(_testContext.ActivePeriod.PeriodNumber);
            clawbackPayment.CollectionPeriodYear.Should().Be(Convert.ToInt16(_testContext.ActivePeriod.AcademicYear));
            clawbackPayment.PaymentId.Should().Be(_payment.Id);
            clawbackPayment.PendingPaymentId.Should().Be(_paidPendingPayment.Id);
            clawbackPayment.SubnominalCode.Should().Be(_payment.SubnominalCode);

            var archivedPendingPayment = archivedPendingPayments.Single();
            archivedPendingPayment.AccountId.Should().Be(_apprenticeshipIncentive.AccountId);
            archivedPendingPayment.AccountLegalEntityId.Should().Be(_apprenticeshipIncentive.AccountLegalEntityId);
            archivedPendingPayment.Amount.Should().Be(_pendingPayment.Amount);
            archivedPendingPayment.ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            archivedPendingPayment.CalculatedDate.Should().BeCloseTo(_pendingPayment.CalculatedDate, new TimeSpan(0, 0, 0, 1));
            archivedPendingPayment.ClawedBack.Should().Be(_pendingPayment.ClawedBack);
            archivedPendingPayment.DueDate.ToLongTimeString().Should().Be(_pendingPayment.DueDate.ToLongTimeString());
            archivedPendingPayment.EarningType.Should().Be(_pendingPayment.EarningType);
            archivedPendingPayment.PaymentYear.Should().Be(_pendingPayment.PaymentYear);
            archivedPendingPayment.PeriodNumber.Should().Be(_pendingPayment.PeriodNumber);

            var archivedValidationResult = archivedPendingPaymentValidationResults.Single();
            archivedValidationResult.PendingPaymentValidationResultId.Should().Be(_pendingPaymentValidationResult.Id);
            archivedValidationResult.PendingPaymentId.Should().Be(_pendingPaymentValidationResult.PendingPaymentId);
            archivedValidationResult.PaymentYear.Should().Be(Convert.ToInt16(_pendingPaymentValidationResult.PaymentYear));
            archivedValidationResult.PeriodNumber.Should().Be(_pendingPaymentValidationResult.PeriodNumber);
            archivedValidationResult.Step.Should().Be(_pendingPaymentValidationResult.Step);
            archivedValidationResult.Result.Should().Be(_pendingPaymentValidationResult.Result);
        }
    }
}
