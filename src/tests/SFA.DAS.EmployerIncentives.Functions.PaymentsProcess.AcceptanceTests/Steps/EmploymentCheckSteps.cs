using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators;
using SFA.DAS.EmployerIncentives.Functions.TestHelpers;
using TechTalk.SpecFlow;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "EmploymentCheck")]
    public class EmploymentCheckSteps
    {
        private readonly TestContext _testContext;
        private readonly Fixture _fixture;
        private readonly ApprenticeshipIncentive _apprenticeshipIncentive;
        private readonly Account _accountModel;
        private DateTime _plannedStartDate;
        private readonly PendingPayment _pendingPayment;
        private readonly LearnerSubmissionDto _learnerMatchApiData;

        public EmploymentCheckSteps(TestContext testContext)
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
                .With(p => p.StartDate, _plannedStartDate)
                .With(p => p.SubmittedDate, _plannedStartDate.AddDays(-30))
                .With(p => p.Phase, Phase.Phase1)
                .Without(x => x.EmploymentChecks)
                .Create();

            _pendingPayment = _fixture.Build<PendingPayment>()
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.DueDate, _plannedStartDate.AddDays(1))
                .With(p => p.ClawedBack, false)
                .With(p => p.EarningType, EarningType.FirstPayment)
                .Without(p => p.PaymentMadeDate)
                .Create();

            _learnerMatchApiData = _fixture
                .Build<LearnerSubmissionDto>()
                .With(s => s.Ukprn, _apprenticeshipIncentive.UKPRN)
                .With(s => s.Uln, _apprenticeshipIncentive.ULN)
                .With(s => s.AcademicYear, "2021")
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

        [Given(@"an apprenticeship incentive has been submitted in phase 1")]
        public async Task GivenAPhase1ApprenticeshipIncentiveExists()
        {
            await CreateIncentive();
        }

        [Given(@"an apprenticeship incentive has been submitted in phase 2")]
        public async Task GivenAPhase2ApprenticeshipIncentiveExists()
        {
            _plannedStartDate = new DateTime(2021, 08, 01);
            _apprenticeshipIncentive.StartDate = _plannedStartDate;
            _apprenticeshipIncentive.Phase = Phase.Phase2;
            await CreateIncentive();
        }
        private async Task CreateIncentive()
        {
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.InsertAsync(_accountModel);
                await dbConnection.InsertAsync(_apprenticeshipIncentive);
                await dbConnection.InsertWithEnumAsStringAsync(_pendingPayment);
            }
        }

        [Given(@"we have not previously requested an employment check for the learner")]
        public void GivenWeHaveNotPreviouslyRequestedAnEmploymentCheckForTheLearner()
        {
        }

        [Given(@"6 weeks has elapsed since the start date of the apprenticeship")]
        public void GivenSixWeeksHasElapsedSinceTheStartDateOfTheApprenticeship()
        {
        }

        [When(@"an ILR submission is received for that learner")]
        public async Task WhenTheIlrSubmissionIsReceived()
        {
            SetupMockLearnerMatchResponse(_learnerMatchApiData);
            await StartLearnerMatching();
        }

        [Then(@"a new employment check is requested to ensure the apprentice was not employed in the 6 months prior to phase 1 starting")]
        public async Task ThenNotEmployedPriorToPhaseStartEmploymentCheckIsCreatedForPhase1()
        {
            VerifyEmployedPriorToPhaseCheck(new DateTime(2020, 08, 01).AddMonths(-6), new DateTime(2020, 07, 31));
        }

        [Then(@"a new employment check is requested to ensure the apprentice was not employed in the 6 months prior to phase 2 starting")]
        public async Task ThenNotEmployedPriorToPhaseStartEmploymentCheckIsCreatedForPhase2()
        {
            VerifyEmployedPriorToPhaseCheck(new DateTime(2021, 4, 1).AddMonths(-6), new DateTime(2021, 3, 31));
        }

        private void VerifyEmployedPriorToPhaseCheck(DateTime minDate, DateTime maxDate)
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var employmentCheck = dbConnection.GetAll<EmploymentCheck>()
                .Single(x => x.CheckType == EmploymentCheckType.EmployedBeforeSchemeStarted);
            employmentCheck.MinimumDate.Should().Be(minDate);
            employmentCheck.MaximumDate.Should().Be(maxDate);
            employmentCheck.Result.Should().BeNull();
        }

        [Then(@"a new employment check is requested to ensure the apprentice was employed in the six weeks following their start date")]
        public async Task ThenEmployedPriorAfterStartDateEmploymentCheckIsCreated()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var employmentCheck = dbConnection.GetAll<EmploymentCheck>().Single(x => x.CheckType == EmploymentCheckType.EmployedAtStartOfApprenticeship);
            employmentCheck.MinimumDate.Should().Be(_plannedStartDate);
            employmentCheck.MaximumDate.Should().Be(_plannedStartDate.AddDays(42));
            employmentCheck.Result.Should().BeNull();
        }

        private void SetupMockLearnerMatchResponse(LearnerSubmissionDto learnerMatchApiData)
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
                    .WithBodyAsJson(learnerMatchApiData));
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
