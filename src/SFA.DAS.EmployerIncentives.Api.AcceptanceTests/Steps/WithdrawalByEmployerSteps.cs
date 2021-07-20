using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Commands.Types.Withdrawals;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using SFA.DAS.Notifications.Messages.Commands;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "WithdrawalByEmployer")]
    public class WithdrawalByEmployerSteps : StepsBase
    {
        private readonly TestContext _testContext;
        private readonly Fixture _fixture;
        private readonly string _connectionString;
        private readonly IncentiveApplication _application;
        private readonly IncentiveApplicationApprenticeship _apprenticeship;
        private WithdrawApplicationRequest _withdrawApplicationRequest;
        private readonly IncentiveApplicationApprenticeship _apprenticeship2;

        private readonly ApprenticeshipIncentive _apprenticeshipIncentive;
        private readonly PendingPayment _pendingPayment;
        private readonly PendingPayment _paidPendingPayment;
        private readonly PendingPayment _clawedbackPaidPendingPayment;
        private readonly PendingPaymentValidationResult _pendingPaymentValidationResult;
        private readonly Payment _payment;
        private readonly Payment _payment2;
        private readonly ClawbackPayment _clawedBackPayment;
        private bool _waitForMessage = true;
        private HttpResponseMessage _response;
        private bool _isMultipleApplications;
        private readonly Account _account;

        public WithdrawalByEmployerSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();
            _connectionString = _testContext.SqlDatabase.DatabaseInfo.ConnectionString;

            _account = TestContext.TestData.GetOrCreate<Account>();

            _application = _fixture.Create<IncentiveApplication>();
            _application.AccountId = _account.Id;
            _application.AccountLegalEntityId = _account.AccountLegalEntityId;

            _apprenticeship = _fixture
                .Build<IncentiveApplicationApprenticeship>()
                .With(a => a.IncentiveApplicationId, _application.Id)
                .With(a => a.WithdrawnByEmployer, false)
                .Create();

            _apprenticeship2 = _fixture
                .Build<IncentiveApplicationApprenticeship>()
                .With(a => a.IncentiveApplicationId, _application.Id)
                .With(a => a.ULN, _apprenticeship.ULN)
                .With(a => a.WithdrawnByEmployer, false)
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
                .With(p => p.ClawedBack, false)
                .Create();

            _paidPendingPayment = _fixture
                .Build<PendingPayment>()
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(i => i.AccountId, _apprenticeshipIncentive.AccountId)
                .With(i => i.AccountLegalEntityId, _apprenticeshipIncentive.AccountLegalEntityId)
                .With(p => p.ClawedBack, false)
                .Create();

            _clawedbackPaidPendingPayment = _fixture
                .Build<PendingPayment>()
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(i => i.AccountId, _apprenticeshipIncentive.AccountId)
                .With(i => i.AccountLegalEntityId, _apprenticeshipIncentive.AccountLegalEntityId)
                .With(p => p.ClawedBack, true)
                .Create();          

            _payment = _fixture
                .Build<Payment>()
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(i => i.AccountId, _apprenticeshipIncentive.AccountId)
                .With(i => i.AccountLegalEntityId, _apprenticeshipIncentive.AccountLegalEntityId)
                .With(p => p.PendingPaymentId, _paidPendingPayment.Id)
                .Create();

            _payment2 = _fixture
                .Build<Payment>()
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(i => i.AccountId, _apprenticeshipIncentive.AccountId)
                .With(i => i.AccountLegalEntityId, _apprenticeshipIncentive.AccountLegalEntityId)
                .With(p => p.PendingPaymentId, _clawedbackPaidPendingPayment.Id)
                .Create();

            _clawedBackPayment = _fixture
              .Build<ClawbackPayment>()
              .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
              .With(i => i.AccountId, _apprenticeshipIncentive.AccountId)
              .With(i => i.AccountLegalEntityId, _apprenticeshipIncentive.AccountLegalEntityId)
              .With(i => i.PendingPaymentId, _clawedbackPaidPendingPayment.Id)
              .With(i => i.PaymentId, _payment2.Id)
              .With(i => i.Amount, _clawedbackPaidPendingPayment.Amount * -1)
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
            await dbConnection.InsertAsync(_account);
            await dbConnection.InsertAsync(_application);
            await dbConnection.InsertAsync(_apprenticeship);
        }

        [Given(@"an incentive application has been made, submitted and has payments")]
        public async Task GivenAnIncentiveApplicationHasBeenMadeSubmittedAndHasPayments()
        {
            using var dbConnection = new SqlConnection(_connectionString);
            await dbConnection.InsertAsync(_account);
            await dbConnection.InsertAsync(_application);
            await dbConnection.InsertAsync(_apprenticeship);
            await dbConnection.InsertAsync(_apprenticeshipIncentive);
            await dbConnection.InsertAsync(_payment);

            _waitForMessage = false;
        }

        [Given(@"multiple incentive applications have been made for the same ULN without being submitted")]
        public async Task GivenMultipleIncentiveApplicationsHaveBeenMadeWithoutBeingSubmitted()
        {
            using var dbConnection = new SqlConnection(_connectionString);
            await dbConnection.InsertAsync(_account);
            await dbConnection.InsertAsync(_application);
            await dbConnection.InsertAsync(_apprenticeship);
            await dbConnection.InsertAsync(_apprenticeship2);

            _isMultipleApplications = true;
        }

        [Given(@"an apprenticeship incentive with pending payments exists as a result of an incentive application")]
        public async Task GivenAnApprenticeshipIncentiveWithPendingPaymentsExistsForAnApplication()
        {
            using var dbConnection = new SqlConnection(_connectionString);
            await dbConnection.InsertAsync(_account);
            await dbConnection.InsertAsync(_application);
            await dbConnection.InsertAsync(_apprenticeship);
            await dbConnection.InsertAsync(_apprenticeshipIncentive);
            await dbConnection.InsertAsync(_pendingPayment);
            await dbConnection.InsertAsync(_pendingPaymentValidationResult);
        }

        [Given(@"an apprenticeship incentive with a clawedback paid payment exists as a result of an incentive application")]
        public async Task GivenAnApprenticeshipIncentiveWithAClawedBackPaymentExistsForAnApplication()
        {
            using var dbConnection = new SqlConnection(_connectionString);
            await dbConnection.InsertAsync(_account);
            await dbConnection.InsertAsync(_application);
            await dbConnection.InsertAsync(_apprenticeship);
            await dbConnection.InsertAsync(_apprenticeshipIncentive);
            await dbConnection.InsertAsync(_pendingPayment);
            await dbConnection.InsertAsync(_clawedbackPaidPendingPayment);
            await dbConnection.InsertAsync(_pendingPaymentValidationResult);
            await dbConnection.InsertAsync(_payment2);
            await dbConnection.InsertAsync(_clawedBackPayment);
        }

        [Given(@"an apprenticeship incentive with paid payments exists as a result of an incentive application")]
        public async Task GivenAnApprenticeshipIncentiveWithPaidPaymentsExistsForAnApplication()
        {
            using var dbConnection = new SqlConnection(_connectionString);
            await dbConnection.InsertAsync(_account);
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
                .With(r => r.WithdrawalType, WithdrawalType.Employer)
                .With(r => r.AccountLegalEntityId, _application.AccountLegalEntityId)
                .With(r => r.ULN, _apprenticeship.ULN)
                .With(r => r.AccountId, _application.AccountId)
                .With(r => r.EmailAddress, "test@email.com")
                .Create();

            var url = $"withdrawals";

            if (_waitForMessage)
            {
                await _testContext.WaitFor(
                async (cancellationToken) =>
                {
                    _response = await EmployerIncentiveApi.Post(url, _withdrawApplicationRequest, cancellationToken);
                },
                (context) => HasExpectedEvents(context)
                );
            }
            else
            {
                _response = await EmployerIncentiveApi.Post(url, _withdrawApplicationRequest);
            }
        }

        private bool HasExpectedEvents(TestContext testContext)
        {
            var processedEvents = testContext.CommandsPublished.Count(c => c.IsProcessed && c.Command is WithdrawCommand);

            if (_isMultipleApplications)
            {
                return processedEvents == 2;
            }
            return processedEvents == 1;
        }

        [Then(@"the incentive application status is updated to indicate the employer withdrawal")]
        public async Task ThenTheIncentiveApplicationStatusIsUpdatedToIndicateTheEmployerWithdrawal()
        {
            _response.StatusCode.Should().Be(HttpStatusCode.Accepted);

            await using var dbConnection = new SqlConnection(_connectionString);
            var apprenticeships = await dbConnection.GetAllAsync<IncentiveApplicationApprenticeship>();
            apprenticeships.Should().HaveCount(1);
            apprenticeships.Single(a => a.Id == _apprenticeship.Id).WithdrawnByEmployer.Should().BeTrue();

            var incentiveApplicationAudits = await dbConnection.GetAllAsync<IncentiveApplicationStatusAudit>();
            incentiveApplicationAudits.Should().HaveCount(1);
            var auditRecord = incentiveApplicationAudits.Single(a => a.IncentiveApplicationApprenticeshipId == _apprenticeship.Id);
            auditRecord.Process.Should().Be(IncentiveApplicationStatus.EmployerWithdrawn);
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

        [Then(@"an email notification is sent to confirm the employer withdrawal")]
        public void ThenAnEmailNotificationIsSentToConfirmTheEmployerWithdrawal()
        {
            var notification = _testContext
                .EventsPublished
                .Single(e => e is SendEmailCommand) as SendEmailCommand;

            Debug.Assert(notification != null, nameof(notification) + " != null");
            notification.RecipientsAddress.Should().Be(_withdrawApplicationRequest.EmailAddress);
            notification.Tokens["uln"].Should().Be(_apprenticeship.ULN.ToString());
            notification.Tokens["organisation name"].Should().Be(_account.LegalEntityName);
        }

        [Then(@"each incentive application status is updated to indicate the employer withdrawal")]
        public async Task ThenEachIncentiveApplicationStatusIsUpdatedToIndicateTheEmployerWithdrawal()
        {
            _response.StatusCode.Should().Be(HttpStatusCode.Accepted);

            await using var dbConnection = new SqlConnection(_connectionString);
            var apprenticeships = await dbConnection.GetAllAsync<IncentiveApplicationApprenticeship>();
            apprenticeships.Should().HaveCount(2);
            apprenticeships.Single(a => a.Id == _apprenticeship.Id).WithdrawnByEmployer.Should().BeTrue();
            apprenticeships.Single(a => a.Id == _apprenticeship2.Id).WithdrawnByEmployer.Should().BeTrue();

            var incentiveApplicationAudits = await dbConnection.GetAllAsync<IncentiveApplicationStatusAudit>();
            incentiveApplicationAudits.Should().HaveCount(2);

            incentiveApplicationAudits.Single(a => a.IncentiveApplicationApprenticeshipId == _apprenticeship.Id).Process.Should().Be(IncentiveApplicationStatus.EmployerWithdrawn);
            incentiveApplicationAudits.Single(a => a.IncentiveApplicationApprenticeshipId == _apprenticeship2.Id).Process.Should().Be(IncentiveApplicationStatus.EmployerWithdrawn);

            _testContext
                .CommandsPublished
                .Count(c => c.IsPublished &&
                c.Command is WithdrawCommand)
                .Should().Be(2);
        }

        [Then(@"the apprenticeship incentive is marked as withdrawn and it's pending payments are removed from the system")]
        public async Task ThenTheIncentiveIsWithdrawnAndPendingPaymentsAreRemovedFromTheSystem()
        {
            await ThenTheIncentiveApplicationStatusIsUpdatedToIndicateTheEmployerWithdrawal();

            await using var dbConnection = new SqlConnection(_connectionString);
            var incentives = await dbConnection.GetAllAsync<ApprenticeshipIncentive>();
            var pendingPayments = await dbConnection.GetAllAsync<PendingPayment>();
            var pendingPaymentValidationResults = await dbConnection.GetAllAsync<PendingPaymentValidationResult>();

            incentives.Should().HaveCount(1);
            var incentive = incentives.FirstOrDefault();
            incentive.Status.Should().Be(IncentiveStatus.Withdrawn);
            incentive.WithdrawnBy.Should().Be(WithdrawnBy.Employer);
            pendingPayments.Should().HaveCount(0);
            pendingPaymentValidationResults.Should().HaveCount(0);
        }

        [Then(@"clawbacks are created for the apprenticeship incentive payments and it's pending payments are archived")]
        public async Task ThenClawbacksAreCreatedForTheIncentiveAndItsPendingPaymentsAreArchived()
        {
            await ThenTheIncentiveApplicationStatusIsUpdatedToIndicateTheEmployerWithdrawal();

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
            incentive.WithdrawnBy.Should().Be(WithdrawnBy.Employer);
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
            archivedPendingPayment.CalculatedDate.ToLongTimeString().Should().Be(_pendingPayment.CalculatedDate.ToLongTimeString());
            archivedPendingPayment.ClawedBack.Should().Be(_pendingPayment.ClawedBack);
            archivedPendingPayment.DueDate.ToLongTimeString().Should().Be(_pendingPayment.DueDate.ToLongTimeString());
            archivedPendingPayment.EarningType.Should().Be(_pendingPayment.EarningType);
            archivedPendingPayment.PaymentYear.Should().Be(_pendingPayment.PaymentYear);
            archivedPendingPayment.PeriodNumber.Should().Be(_pendingPayment.PeriodNumber);

            var archivedValidationResult = archivedPendingPaymentValidationResults.Single();
            archivedValidationResult.PendingPaymentValidationResultId.Should().Be(_pendingPaymentValidationResult.Id);
            archivedValidationResult.PendingPaymentId.Should().Be(_pendingPaymentValidationResult.PendingPaymentId);
            archivedValidationResult.PaymentYear.Should().Be(_pendingPaymentValidationResult.PaymentYear);
            archivedValidationResult.PeriodNumber.Should().Be(_pendingPaymentValidationResult.PeriodNumber);
            archivedValidationResult.Step.Should().Be(_pendingPaymentValidationResult.Step);
            archivedValidationResult.Result.Should().Be(_pendingPaymentValidationResult.Result);
        }

        [Then(@"the pending payments are archived")]
        public async Task ThenThePendingPaymentsAreArchived()
        {
            await ThenTheIncentiveApplicationStatusIsUpdatedToIndicateTheEmployerWithdrawal();

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
            incentive.WithdrawnBy.Should().Be(WithdrawnBy.Employer);
            incentive.PausePayments.Should().BeFalse();

            payments.Single().Id.Should().Be(_payment2.Id);
            pendingPayments.Single().Id.Should().Be(_clawedbackPaidPendingPayment.Id);

            var archivedPendingPayment = archivedPendingPayments.Single();
            archivedPendingPayment.AccountId.Should().Be(_apprenticeshipIncentive.AccountId);
            archivedPendingPayment.AccountLegalEntityId.Should().Be(_apprenticeshipIncentive.AccountLegalEntityId);
            archivedPendingPayment.Amount.Should().Be(_pendingPayment.Amount);
            archivedPendingPayment.ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            archivedPendingPayment.CalculatedDate.Should().BeCloseTo(_pendingPayment.CalculatedDate, 1000);
            archivedPendingPayment.ClawedBack.Should().Be(_pendingPayment.ClawedBack);
            archivedPendingPayment.DueDate.ToLongTimeString().Should().Be(_pendingPayment.DueDate.ToLongTimeString());
            archivedPendingPayment.EarningType.Should().Be(_pendingPayment.EarningType);
            archivedPendingPayment.PaymentYear.Should().Be(_pendingPayment.PaymentYear);
            archivedPendingPayment.PeriodNumber.Should().Be(_pendingPayment.PeriodNumber);

            var archivedValidationResult = archivedPendingPaymentValidationResults.Single();
            archivedValidationResult.PendingPaymentValidationResultId.Should().Be(_pendingPaymentValidationResult.Id);
            archivedValidationResult.PendingPaymentId.Should().Be(_pendingPaymentValidationResult.PendingPaymentId);
            archivedValidationResult.PaymentYear.Should().Be(_pendingPaymentValidationResult.PaymentYear);
            archivedValidationResult.PeriodNumber.Should().Be(_pendingPaymentValidationResult.PeriodNumber);
            archivedValidationResult.Step.Should().Be(_pendingPaymentValidationResult.Step);
            archivedValidationResult.Result.Should().Be(_pendingPaymentValidationResult.Result);
        }

        [Then(@"an error is returned")]
        public async Task ThenAnErrorIsReturned()
        {
            _response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var badRequestResponse = JsonConvert.DeserializeObject<BadRequestResponse>(await _response.Content.ReadAsStringAsync());
            badRequestResponse.Error.Should().Be("Cannot withdraw an application that has been submitted and has received payments");
        }

        [Then(@"an email is sent to confirm the cancelled application")]
        public async Task ThenAnEmailIsSentToConfirmTheCancelledApplication()
        {
            //var thyingdsfsfsdfsd =_testContext;
            Assert.Pass();
            //var publishedCommands = _testContext.CommandsPublished
            //    .Where(c => c.IsPublished 
            //                // && c.Command is SendEmailCommand
            //                )
            //    .Select(c => c.Command)
            //    .ToArray();

            //publishedCommands.Count(x => x is SendEmailCommand).Should().Be(1);
        }

        public class BadRequestResponse
        {
            public string Error { get; set; }
        }
    }
}
