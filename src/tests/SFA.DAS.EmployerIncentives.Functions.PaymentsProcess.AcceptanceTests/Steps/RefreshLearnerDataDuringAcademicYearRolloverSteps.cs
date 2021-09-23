using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators;
using SFA.DAS.EmployerIncentives.Functions.TestHelpers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "RefreshLearnerDataDuringAcademicYearRollover")]
    public class RefreshLearnerDataDuringAcademicYearRolloverSteps
    {
        private readonly TestContext _context;
        private readonly Data.Models.Account _account;
        private readonly Fixture _fixture;
        private readonly ApprenticeshipIncentive _incentive;
        private readonly LearnerSubmissionDto _lerner;
        private DateTime _endDate;
        public RefreshLearnerDataDuringAcademicYearRolloverSteps(TestContext context)
        {
            _context = context;
            _fixture = new Fixture();
            _account = _fixture.Create<Data.Models.Account>();
            var start = DateTime.Parse("2021-04-15");

            _incentive = _fixture.Build<ApprenticeshipIncentive>()
                .With(p => p.DateOfBirth, start.AddYears(-26))
                .With(p => p.AccountId, _account.Id)
                .With(p => p.AccountLegalEntityId, _account.AccountLegalEntityId)
                .With(p => p.StartDate, start)
                .Without(p => p.PendingPayments)
                .Without(p => p.Payments)
                .With(p => p.Phase, Phase.Phase2)
                .Create();

            _lerner = _fixture
                .Build<LearnerSubmissionDto>()
                .With(s => s.Ukprn, _incentive.UKPRN)
                .With(s => s.Uln, _incentive.ULN)
                .With(s => s.AcademicYear, "2021")
                .With(l => l.Training, new List<TrainingDto> {
                    _fixture
                        .Build<TrainingDto>()
                        .With(p => p.Reference, "ZPROG001")
                        .With(p => p.PriceEpisodes, new List<PriceEpisodeDto>{
                            _fixture.Build<PriceEpisodeDto>()
                                .With(x => x.AcademicYear,"2021")
                                .With(x => x.StartDate, DateTime.Parse("2020-08-01T00:00:00"))
                                .With(x => x.EndDate, DateTime.Parse("2021-07-31T00:00:00"))
                                .With(pe => pe.Periods, new List<PeriodDto>{
                                    _fixture.Build<PeriodDto>()
                                        .With(period => period.ApprenticeshipId, _incentive.ApprenticeshipId)
                                        .With(period => period.IsPayable, true)
                                        .With(period => period.Period, 10)
                                        .Create(),
                                    _fixture.Build<PeriodDto>()
                                        .With(period => period.ApprenticeshipId, _incentive.ApprenticeshipId)
                                        .With(period => period.IsPayable, true)
                                        .With(period => period.Period, 11)
                                        .Create(),
                                    _fixture.Build<PeriodDto>()
                                        .With(period => period.ApprenticeshipId, _incentive.ApprenticeshipId)
                                        .With(period => period.IsPayable, true)
                                        .With(period => period.Period, 12)
                                        .Create()
                                })
                            .Create() }
                        )
                        .Create()}
                )
                .Create();
        }


        [Given(@"a successful learner match response for an incentive application")]
        public async Task GivenASuccessfulLearnerMatchResponseForAnIncentiveApplication()
        {
            await using var dbConnection = new SqlConnection(_context.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(_account);
            await dbConnection.InsertAsync(_incentive);

            SetupMockLearnerMatchResponse();
            await StartLearnerMatching();
        }

        [When(@"the most recent price episode has no periods")]
        public void WhenTheMostRecentPriceEpisodeHasNoPeriods()
        {
            _endDate = DateTime.Parse("2021-08-10T00:00:00");
            var episode = _fixture.Build<PriceEpisodeDto>()
                .With(x => x.AcademicYear, "2122")
                .With(x => x.StartDate, DateTime.Parse("2021-08-01T00:00:00"))
                .With(x => x.EndDate, _endDate)
                .With(pe => pe.Periods, new List<PeriodDto>())
                .Create();

            _lerner.Training.First().PriceEpisodes.Add(episode);
        }

        [When(@"the previous price episode has a period with a matching apprenticeship ID")]
        public void WhenThePreviousPriceEpisodeHasAPeriodWithAMatchingApprenticeshipId()
        {
            // intentionally blank
        }
        
        [Then(@"trigger a learning stopped Change of Circumstance")]
        public async Task ThenTriggerALearningStoppedChangeOfCircumstance()
        {
            _lerner.IlrSubmissionDate = _lerner.IlrSubmissionDate.AddMonths(1);
            SetupMockLearnerMatchResponse();
            await StartLearnerMatching();
        }
        
        [Then(@"record the learning stopped date as the day after the last price episode end date")]
        public async Task ThenRecordTheLearningStoppedDateAsTheDayAfterTheLastPriceEpisodeEndDate()
        {
            await using var dbConnection = new SqlConnection(_context.SqlDatabase.DatabaseInfo.ConnectionString);
            var learner = (await dbConnection.GetAllAsync<Learner>()).Single(l => l.ApprenticeshipIncentiveId == _incentive.Id);

            var expectedStopDate = _endDate.AddDays(1);
            learner.LearningStoppedDate.Should().Be(expectedStopDate);
        }

        private void SetupMockLearnerMatchResponse()
        {
            _context.LearnerMatchApi.MockServer
                .Given(
                    Request
                        .Create()
                        .WithPath($"/api/v1.0/{_incentive.UKPRN}/{_incentive.ULN}")
                        .UsingGet()
                )
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(_lerner));
        }

        private async Task StartLearnerMatching()
        {
            await _context.TestFunction.Start(
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
