using AutoFixture;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators;
using SFA.DAS.EmployerIncentives.Functions.TestHelpers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Steps
{
    public partial class IlrStartDateChangedSteps
    {
        private readonly TestContext _testContext;
        private readonly Account _accountModel;
        private readonly Fixture _fixture;
        private readonly ApprenticeshipIncentive _apprenticeshipIncentive;
        private readonly PendingPayment _pendingPayment;
        private readonly LearnerSubmissionDto _learnerMatchApiData;
        private readonly DateTime _plannedStartDate;
        private readonly PendingPaymentValidationResult _pendingPaymentValidationResult;
        private Payment _payment;
        private List<PendingPayment> _newPendingPayments;
        private DateTime _actualStartDate;
        private readonly DateTime _initialStartDate;
        private readonly (byte Number, short Year) _paymentPeriod = (1, 2021);

        public IlrStartDateChangedSteps(TestContext testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();

            _plannedStartDate = new DateTime(2020, 8, 1);
            _accountModel = _fixture.Create<Account>();

            _apprenticeshipIncentive = _fixture.Build<ApprenticeshipIncentive>()
                .With(p => p.DateOfBirth, _plannedStartDate.AddYears(-24).AddMonths(-10)) // under 25
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
                .With(p => p.HasPossibleChangeOfCircumstances, false)
                .Create();

            _initialStartDate = _apprenticeshipIncentive.StartDate;

            _pendingPayment = _fixture.Build<PendingPayment>()
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.AccountLegalEntityId, _apprenticeshipIncentive.AccountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.DueDate, _plannedStartDate.AddMonths(1))
                .With(p => p.PeriodNumber, _paymentPeriod.Number)
                .With(p => p.PaymentYear, _paymentPeriod.Year)
                .With(p => p.Amount, 750)
                .With(p => p.ClawedBack, false)
                .With(p => p.EarningType, EarningType.FirstPayment)
                .Without(p => p.PaymentMadeDate)
                .Create();

            _pendingPaymentValidationResult = _fixture.Build<PendingPaymentValidationResult>()
                .With(p => p.PendingPaymentId, _pendingPayment.Id)
                .With(p => p.PeriodNumber, _paymentPeriod.Number)
                .With(p => p.PaymentYear, _paymentPeriod.Year)
                .With(p => p.Step, "HasBankDetails")
                .With(p => p.Result, true)
                .Create();

            _learnerMatchApiData = _fixture
                .Build<LearnerSubmissionDto>()
                .With(s => s.Ukprn, _apprenticeshipIncentive.UKPRN)
                .With(s => s.Uln, _apprenticeshipIncentive.ULN)
                .With(l => l.Training, new List<TrainingDto> {
                    _fixture
                        .Build<TrainingDto>()
                        .With(p => p.Reference, "ZPROG001")
                        .With(p => p.PriceEpisodes, new List<PriceEpisodeDto>(){_fixture.Build<PriceEpisodeDto>()
                            .With(pe => pe.Periods, new List<PeriodDto>(){
                                _fixture.Build<PeriodDto>()
                                    .With(period => period.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                                    .With(period => period.IsPayable, true)
                                    .With(period => period.Period, _pendingPayment.PeriodNumber)
                                    .Create()
                            })
                            .With(pe => pe.StartDate, _plannedStartDate)
                            .With(pe => pe.EndDate, _plannedStartDate.AddYears(1))
                            .Create() }
                        )
                        .Create()}
                )
                .Create();
        }

        private void SetupMockLearnerMatchResponse()
        {
            _testContext.LearnerMatchApi.MockServer
                .Given(
                    Request
                        .Create()
                        .WithPath($"/api/v1.0/{_apprenticeshipIncentive.UKPRN}/{_apprenticeshipIncentive.ULN}")
                        .UsingGet()
                )
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(_learnerMatchApiData));
        }

        private async Task StartLearnerMatching()
        {
            await _testContext.TestFunction.Start(
                new OrchestrationStarterInfo(
                    "LearnerMatchingOrchestrator_Start",
                    nameof(LearnerMatchingOrchestrator),
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
