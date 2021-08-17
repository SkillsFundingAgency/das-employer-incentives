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
        private readonly Fixture _fixture;
        private Account _accountModel;
        private  ApprenticeshipIncentive _apprenticeshipIncentive;
        private  PendingPayment _pendingPayment;
        private  LearnerSubmissionDto _learnerMatchApiData;
        private DateTime _plannedStartDate;
        private  PendingPaymentValidationResult _pendingPaymentValidationResult;
        private Payment _payment;
        private List<PendingPayment> _newPendingPayments;
        private DateTime _actualStartDate;
        private Phase _phase;
        private readonly (byte Number, short Year) _paymentPeriod = (1, 2021);

        public IlrStartDateChangedSteps(TestContext testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();
        }

        private void CreateIncentive(string phase)
        {
            _phase = Enum.Parse<Phase>(phase);
            _plannedStartDate = (_phase == Phase.Phase1) ? new DateTime(2020, 8, 1) : new DateTime(2021, 7, 1);
            
            _accountModel = _fixture.Create<Account>();

            _apprenticeshipIncentive = _fixture.Build<ApprenticeshipIncentive>()
                .With(p => p.DateOfBirth, _plannedStartDate.AddYears(-24).AddMonths(-10)) // under 25
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
                .With(p => p.HasPossibleChangeOfCircumstances, false)
                .With(p => p.Phase, _phase)
                .With(p => p.MinimumAgreementVersion, 1)
                .With(p => p.StartDate, _plannedStartDate)
                .With(p => p.SubmittedDate, _plannedStartDate.AddDays(-30))
                .With(p => p.Status, IncentiveStatus.Active)
                .Create();


            _pendingPayment = _fixture.Build<PendingPayment>()
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.AccountLegalEntityId, _apprenticeshipIncentive.AccountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.DueDate, _plannedStartDate.AddMonths(1))
                .With(p => p.PeriodNumber, _paymentPeriod.Number)
                .With(p => p.PaymentYear, _paymentPeriod.Year)
                .With(p => p.Amount, 1000)
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
                        .With(p => p.PriceEpisodes, new List<PriceEpisodeDto>(){_fixture.Build<PriceEpisodeDto>().With(x => x.AcademicYear,"2021")
                            .With(pe => pe.Periods, new List<PeriodDto>(){
                                _fixture.Build<PeriodDto>()
                                    .With(period => period.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                                    .With(period => period.IsPayable, true)
                                    .With(period => period.Period, _pendingPayment.PeriodNumber)
                                    .Create()
                            })
                            .With(pe => pe.StartDate, _plannedStartDate)
                            .With(pe => pe.EndDate, _plannedStartDate.AddYears(3))
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
