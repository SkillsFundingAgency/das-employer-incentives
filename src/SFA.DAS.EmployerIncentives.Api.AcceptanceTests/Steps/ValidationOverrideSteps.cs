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

        public ValidationOverrideSteps(TestContext testContext) : base(testContext)
        {
            _account = TestContext.TestData.GetOrCreate<Account>();
            _application = Fixture.Create<IncentiveApplication>();

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
        }

        [Given(@"an apprenticeship incentive has a (.*) validation")]
        public async Task GivenAnApprenticeshipIncentiveHasAValidation(ValidationType validationType)
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
                new List<ValidationOverride>() {
                    Fixture.Build<ValidationOverride>()
                    .With(o => o.AccountLegalEntityId, _apprenticeshipIncentive.AccountLegalEntityId)
                    .With(o => o.ULN, _apprenticeshipIncentive.ULN)
                    .With(o => o.ValidationSteps, new List<ValidationStep>(){
                        Fixture.Build<ValidationStep>()
                        .With(v => v.ValidationType, Enum.Parse<ValidationType>(_pendingPaymentValidationResult.Step))
                        .With(v => v.ExpiryDate, DateTime.Today.AddDays(1))
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

        private bool HasExpectedEvents(TestContext testContext)
        {
            var processedEvents = testContext.CommandsPublished.Count(c =>
            c.IsProcessed &&
            c.Command is ValidationOverrideCommand);

            return processedEvents == 1;
        }

        [Then(@"the validation override (.*) is stored against the apprenticeship incentive")]
        public async Task ThenTheValidationOverrideIsStoredAgainstTheApprenticeshipIncentive(ValidationType validationType)
        {
            _response.StatusCode.Should().Be(HttpStatusCode.Accepted);

            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            // TODO: change to ValidationOverride and assert expirey date and service request fields etc..
            var validationOverrides = await dbConnection.GetAllAsync<PendingPaymentValidationResult>();
            validationOverrides.Should().HaveCount(1);
            //validationOverrides.Single(a => a.Step == validationType.ToString());

        }
    }
}
