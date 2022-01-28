using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.ValidationOverrides;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "ValidationOverride")]
    public class ValidationOverrideSteps : StepsBase
    {
        private HttpResponseMessage _response;
        private ValidationOverrideRequest _validationOverrideRequest;

        private readonly Account _account;
        private readonly IncentiveApplication _application;
        private readonly IncentiveApplicationApprenticeship _applicationApprenticeship;
        private readonly ApprenticeshipIncentive _apprenticeshipIncentive;
        private readonly PendingPayment _pendingPayment;
        private readonly PendingPaymentValidationResult _pendingPaymentValidationResult;
        private readonly Data.ApprenticeshipIncentives.Models.ValidationOverride _validationOverride;
        private readonly ValidationOverrideAudit _validationOverrideAudit;
        private readonly DateTime _expiryDate;        

        public ValidationOverrideSteps(TestContext testContext) : base(testContext)
        {
            _account = TestContext.TestData.GetOrCreate<Account>();
            _application = Fixture.Create<IncentiveApplication>();
            _expiryDate = DateTime.Today.AddDays(1);

            _applicationApprenticeship = Fixture
                .Build<IncentiveApplicationApprenticeship>()
                .With(a => a.IncentiveApplicationId, _application.Id)
                .Create();

            _apprenticeshipIncentive = Fixture
                .Build<ApprenticeshipIncentive>()
                .With(i => i.IncentiveApplicationApprenticeshipId, _applicationApprenticeship.Id)
                .With(a => a.ULN, _applicationApprenticeship.ULN)
                .With(a => a.AccountLegalEntityId, _application.AccountLegalEntityId)
                .Create();

            _pendingPayment = Fixture
               .Build<PendingPayment>()
               .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
               .With(i => i.AccountId, _apprenticeshipIncentive.AccountId)
               .With(i => i.AccountLegalEntityId, _apprenticeshipIncentive.AccountLegalEntityId)
               .With(p => p.PaymentMadeDate, (DateTime?)null)
               .With(p => p.ClawedBack, false)
               .Create();

            _pendingPaymentValidationResult = Fixture
                .Build<PendingPaymentValidationResult>()
                .With(p => p.PendingPaymentId, _pendingPayment.Id)
                .With(p => p.Step, ValidationType.HasDaysInLearning.ToString())
                .With(p => p.PeriodNumber, 1)
                .With(p => p.PaymentYear, 2021)
                .With(p => p.Result, false)
                .Create();

            _validationOverride = Fixture
                .Build<Data.ApprenticeshipIncentives.Models.ValidationOverride>()
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.Step, ValidationType.IsInLearning.ToString())
                .With(p => p.ExpiryDate, DateTime.Today.AddDays(5))
                .With(p => p.CreatedDateTime, DateTime.Today.AddDays(-1))
                .Create();

            _validationOverrideAudit = Fixture
                .Build<ValidationOverrideAudit>()
                .With(p => p.Id, _validationOverride.Id)
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.Step, ValidationType.IsInLearning.ToString())
                .With(p => p.ExpiryDate, DateTime.Today.AddDays(5))
                .With(p => p.CreatedDateTime, DateTime.Today.AddDays(-1))
                .Without(p => p.DeletedDateTime)
                .Create();
        }

        [Given(@"an apprenticeship incentive has a validation")]
        public async Task GivenAnApprenticeshipIncentiveHasAValidation()
        {
            await GivenAnApprenticeshipIncentiveHasAValidationWithType(ValidationType.IsInLearning);
        }

        [Given(@"an apprenticeship incentive has multiple validations")]
        public async Task GivenAnApprenticeshipIncentivesHasMultipleValidations()
        {
            await GivenAnApprenticeshipIncentiveHasAValidationWithType(ValidationType.IsInLearning);

            var pendingPaymentValidationResult = Fixture
                .Build<PendingPaymentValidationResult>()
                .With(p => p.PendingPaymentId, _pendingPayment.Id)
                .With(p => p.Step, ValidationType.HasDaysInLearning.ToString())
                .With(p => p.PeriodNumber, 1)
                .With(p => p.PaymentYear, 2021)
                .With(p => p.Result, false)
                .Create();

            using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(pendingPaymentValidationResult);
        }

        [Given(@"an apprenticeship incentive has a validation override")]
        public async Task GivenAnApprenticeshipIncentiveHasAValidationOverride()
        {        
            await GivenAnApprenticeshipIncentiveHasAValidation();
            
            using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(_validationOverride);
            await dbConnection.InsertAsync(_validationOverrideAudit);            
        }

        [Given(@"an apprenticeship incentive has a (.*) validation")]
        public async Task GivenAnApprenticeshipIncentiveHasAValidationWithType(ValidationType validationType)
        {
            _pendingPaymentValidationResult.Step = validationType.ToString();

            using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(_account);
            await dbConnection.InsertAsync(_application);
            await dbConnection.InsertAsync(_applicationApprenticeship);
            await dbConnection.InsertAsync(_apprenticeshipIncentive);
            await dbConnection.InsertAsync(_pendingPayment);
            await dbConnection.InsertAsync(_pendingPaymentValidationResult);
        }

        [When(@"the validation override request is received")]
        public async Task WhenTheValidationOverrideRequestIsReceived()
        {
            _validationOverrideRequest = Fixture
                .Build<ValidationOverrideRequest>()
                .With(p => p.ValidationOverrides,
                new List<Types.ValidationOverride>() {

                    Fixture.Build<Types.ValidationOverride>()
                    .With(o => o.AccountLegalEntityId, _apprenticeshipIncentive.AccountLegalEntityId)
                    .With(o => o.ULN, _apprenticeshipIncentive.ULN)
                    .With(o => o.ValidationSteps, new List<ValidationStep>(){
                        Fixture.Build<ValidationStep>()
                        .With(v => v.ValidationType, Enum.Parse<ValidationType>(_pendingPaymentValidationResult.Step))
                        .With(v => v.ExpiryDate, _expiryDate)
                        .Create()
                    }.ToArray())
                    .Create()
                }.ToArray())
                .Create();

            var url = $"validation-overrides";

            await TestContext.WaitFor(
                async (cancellationToken) =>
                {
                    _response = await EmployerIncentiveApi.Post(url, _validationOverrideRequest, cancellationToken);
                },
                (context) => HasExpectedEvents(context)
                );
        }

        [When(@"the multiple validation override request is received")]
        public async Task WhenTheMultipleValidationOverrideRequestIsReceived()
        {
            _validationOverrideRequest = Fixture
                .Build<ValidationOverrideRequest>()
                .With(p => p.ValidationOverrides,
                new List<Types.ValidationOverride>() {

                    Fixture.Build<Types.ValidationOverride>()
                    .With(o => o.AccountLegalEntityId, _apprenticeshipIncentive.AccountLegalEntityId)
                    .With(o => o.ULN, _apprenticeshipIncentive.ULN)
                    .With(o => o.ValidationSteps, new List<ValidationStep>(){
                        Fixture.Build<ValidationStep>()
                        .With(v => v.ValidationType, ValidationType.IsInLearning)
                        .With(v => v.ExpiryDate, _expiryDate)
                        .Create()
                    }.ToArray())
                    .Create(),

                    Fixture.Build<Types.ValidationOverride>()
                    .With(o => o.AccountLegalEntityId, _apprenticeshipIncentive.AccountLegalEntityId)
                    .With(o => o.ULN, _apprenticeshipIncentive.ULN)
                    .With(o => o.ValidationSteps, new List<ValidationStep>(){
                        Fixture.Build<ValidationStep>()
                        .With(v => v.ValidationType, ValidationType.HasLearningRecord)
                        .With(v => v.ExpiryDate, _expiryDate)
                        .Create()
                    }.ToArray())
                    .Create()

                }.ToArray())
                .Create();

            var url = $"validation-overrides";

            await TestContext.WaitFor(
                async (cancellationToken) =>
                {
                    _response = await EmployerIncentiveApi.Post(url, _validationOverrideRequest, cancellationToken);
                },
                (context) => HasExpectedEvents(context)
                );
        }

        [When(@"the validation override request is received for a non matching apprenticeship incentive")]
        public async Task WhenTheValidationOverrideRequestIsReceivedForANonMatchingApprenticeshipIncentive()
        {
            _validationOverrideRequest = Fixture
                .Build<ValidationOverrideRequest>()
                .With(p => p.ValidationOverrides,
                new List<Types.ValidationOverride>() {
                    Fixture.Build<Types.ValidationOverride>()
                    .With(o => o.AccountLegalEntityId, _apprenticeshipIncentive.AccountLegalEntityId)
                    .With(o => o.ValidationSteps, new List<ValidationStep>(){
                        Fixture.Build<ValidationStep>()
                        .With(v => v.ValidationType, Enum.Parse<ValidationType>(_pendingPaymentValidationResult.Step))
                        .With(v => v.ExpiryDate, _expiryDate)
                        .Create()
                    }.ToArray())
                    .Create()
                }.ToArray())
                .Create();

            var url = $"validation-overrides";

            await TestContext.WaitFor(
                async (cancellationToken) =>
                {
                    _response = await EmployerIncentiveApi.Post(url, _validationOverrideRequest, cancellationToken);
                },
                (context) => HasErroredEvents(context),
                assertOnError: false
                );
        }

        private bool HasExpectedEvents(TestContext testContext)
        {
            var processedEvents = testContext.CommandsPublished.Count(c =>
            c.IsProcessed &&
            c.Command is ValidationOverrideCommand);

            return processedEvents == 1;
        }

        private bool HasErroredEvents(TestContext testContext)
        {
            var processedEvents = testContext.CommandsPublished.Count(c =>
            c.IsErrored &&
            c.Command is ValidationOverrideCommand);

            return processedEvents == 1;
        }

        [Then(@"the validation override is not stored against the apprenticeship incentive")]
        public async Task ThenTheValidationOverrideIsNotStoredAgainstTheApprenticeshipIncentive()
        {
            await ThenTheValidationOverrideIsNotStoredAgainstTheApprenticeshipIncentiveWithType(ValidationType.IsInLearning);
        }

        [Then(@"the validation override (.*) is stored against the apprenticeship incentive")]
        public async Task ThenTheValidationOverrideIsStoredAgainstTheApprenticeshipIncentiveWithType(ValidationType validationType)
        {
            _response.StatusCode.Should().Be(HttpStatusCode.Accepted);

            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);

            var validationOverrides = await dbConnection.GetAllAsync<Data.ApprenticeshipIncentives.Models.ValidationOverride>();
            var validationOverride = validationOverrides.Single(o => o.Step == validationType.ToString());
            validationOverride.ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            validationOverride.ExpiryDate.Should().Be(_expiryDate);
        }

        [Then(@"the validation override is stored against the apprenticeship incentive")]
        public async Task ThenTheValidationOverrideIsStoredAgainstTheApprenticeshipIncentive()
        {
            await ThenTheValidationOverrideIsStoredAgainstTheApprenticeshipIncentiveWithType(ValidationType.IsInLearning);
        }

        [Then(@"the validation override (.*) is not stored against the apprenticeship incentive")]
        public async Task ThenTheValidationOverrideIsNotStoredAgainstTheApprenticeshipIncentiveWithType(ValidationType validationType)
        {
            _response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);            
            var validationOverrides = await dbConnection.GetAllAsync<Data.ApprenticeshipIncentives.Models.ValidationOverride>();
            validationOverrides.Should().HaveCount(0);
        }

        [Then(@"the existing validation override is archived")]
        public async Task ThenTheExistingValidationOverrideIsArchived()
        {
            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);

            var audits = await dbConnection.GetAllAsync<ValidationOverrideAudit>();
            audits.Should().HaveCount(2);

            var deletedAudit = audits.Single(a => a.Id == _validationOverrideAudit.Id);
            deletedAudit.ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            deletedAudit.Step.Should().Be(_validationOverrideAudit.Step);
            deletedAudit.ExpiryDate.Should().Be(_validationOverrideAudit.ExpiryDate);
            deletedAudit.CreatedDateTime.Should().Be(_validationOverrideAudit.CreatedDateTime);
            deletedAudit.DeletedDateTime.Should().BeCloseTo(DateTime.UtcNow, new TimeSpan(0, 2, 0));

            var addedAudit = audits.Single(a => a.DeletedDateTime == null);
            addedAudit.ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            addedAudit.Step.Should().Be(_pendingPaymentValidationResult.Step);
            addedAudit.ExpiryDate.Should().Be(_expiryDate);
            addedAudit.CreatedDateTime.Should().BeCloseTo(DateTime.UtcNow, new TimeSpan(0, 2, 0));
        }

        [Then(@"the validation overrides are stored against the apprenticeship incentives")]
        public async Task ThenTheValidationOverridesAreStoredAgainstTheApprenticeshipIncentives()
        {
            await ThenTheValidationOverrideIsStoredAgainstTheApprenticeshipIncentiveWithType(ValidationType.HasLearningRecord);
            await ThenTheValidationOverrideIsStoredAgainstTheApprenticeshipIncentiveWithType(ValidationType.IsInLearning);
        }
    }
}
