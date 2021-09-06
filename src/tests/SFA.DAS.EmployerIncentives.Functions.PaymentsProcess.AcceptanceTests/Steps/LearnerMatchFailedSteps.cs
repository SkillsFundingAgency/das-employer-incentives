using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
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
    public class LearnerMatchFailedSteps
    {
        private readonly TestContext _testContext;
        private readonly Fixture _fixture;
        private readonly Account _accountModel;
        private readonly ApprenticeshipIncentive _apprenticeshipIncentive1;
        private readonly ApprenticeshipIncentive _apprenticeshipIncentive2;
        private readonly Learner _learner1;
        private readonly Learner _learner2;
        private readonly DateTime _startDate;
        private readonly byte _periodNumber;

        public LearnerMatchFailedSteps(TestContext testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();
            _periodNumber = 1;

            _startDate = new DateTime(2021, 7, 1);
            _accountModel = _fixture.Create<Account>();

            _apprenticeshipIncentive1 = _fixture.Build<ApprenticeshipIncentive>()
                .With(p => p.DateOfBirth, new DateTime(1995, 10, 15))
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
                .With(p => p.HasPossibleChangeOfCircumstances, false)
                .With(p => p.StartDate, _startDate)
                .With(p => p.SubmittedDate, _startDate.AddDays(-30))
                .With(p => p.RefreshedLearnerForEarnings, true)
                .With(p => p.PausePayments, false)
                .With(p => p.Status, IncentiveStatus.Active)
                .With(p => p.BreakInLearnings, new List<ApprenticeshipBreakInLearning>())
                .With(p => p.Phase, Phase.Phase2)
                .Create();

            _apprenticeshipIncentive2 = _fixture.Build<ApprenticeshipIncentive>()
                .With(p => p.DateOfBirth, new DateTime(1996, 11, 16))
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
                .With(p => p.HasPossibleChangeOfCircumstances, false)
                .With(p => p.StartDate, _startDate)
                .With(p => p.SubmittedDate, _startDate.AddDays(-30))
                .With(p => p.RefreshedLearnerForEarnings, true)
                .With(p => p.PausePayments, false)
                .With(p => p.Status, IncentiveStatus.Active)
                .With(p => p.BreakInLearnings, new List<ApprenticeshipBreakInLearning>())
                .With(p => p.Phase, Phase.Phase2)
                .Create();

            _learner1 = _fixture
                .Build<Learner>()
                .With(s => s.Ukprn, _apprenticeshipIncentive1.UKPRN)
                .With(s => s.ULN, _apprenticeshipIncentive1.ULN)
                .With(s => s.StartDate, _apprenticeshipIncentive1.StartDate)
                .With(s => s.ApprenticeshipId, _apprenticeshipIncentive1.ApprenticeshipId)
                .With(s => s.ApprenticeshipIncentiveId, _apprenticeshipIncentive1.Id)
                .With(s => s.HasDataLock, false)
                .With(s => s.SubmissionFound, true)
                .With(s => s.SuccessfulLearnerMatch, true)
                .With(s => s.InLearning, true)
                .With(s => s.LearningFound, true)
                .Without(s => s.LearningStoppedDate)
                .Without(s => s.LearningResumedDate)
                .With(l => l.LearningPeriods, new List<LearningPeriod> {
                    _fixture
                        .Build<LearningPeriod>()
                        .With(p => p.StartDate, _startDate)
                        .Without(p => p.EndDate)
                            .Create()})
                .Create();

            _learner2 = _fixture
                .Build<Learner>()
                .With(s => s.Ukprn, _apprenticeshipIncentive2.UKPRN)
                .With(s => s.ULN, _apprenticeshipIncentive2.ULN)
                .With(s => s.ApprenticeshipId, _apprenticeshipIncentive2.ApprenticeshipId)
                .With(s => s.ApprenticeshipIncentiveId, _apprenticeshipIncentive2.Id)
                .With(s => s.HasDataLock, false)
                .With(s => s.SubmissionFound, true)
                .With(s => s.SuccessfulLearnerMatch, true)
                .With(s => s.InLearning, true)
                .With(s => s.LearningFound, true)
                .Without(s => s.LearningStoppedDate)
                .Without(s => s.LearningResumedDate)
                .With(l => l.LearningPeriods, new List<LearningPeriod> {
                    _fixture
                        .Build<LearningPeriod>()
                        .With(p => p.StartDate, _startDate)
                        .Without(p => p.EndDate)
                            .Create()})
                .Create();
        }

        [Given(@"existing apprenticeship incentives")]
        public async Task GivenExistingLearnerDataSuccessfullyUpdatedInThePast()
        {
            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(_accountModel);
            await dbConnection.InsertAsync(_apprenticeshipIncentive1);
            await dbConnection.InsertAsync(_apprenticeshipIncentive2);
            await dbConnection.InsertAsync(_learner1);
            await dbConnection.InsertAsync(_learner2);
        }

        [Given(@"the learner match process has been triggered")]
        public void GivenTheLearnerMatchProcessHasBeenTriggered()
        {
            SetupMockLearnerMatchErrorResponse(_apprenticeshipIncentive1.UKPRN.Value, _apprenticeshipIncentive1.ULN);

            // no change
            var learner2Data = _fixture
                .Build<LearnerSubmissionDto>()
                .With(s => s.Ukprn, _apprenticeshipIncentive2.UKPRN)
                .With(s => s.Uln, _apprenticeshipIncentive2.ULN)
                .With(s => s.StartDate, _apprenticeshipIncentive2.StartDate)
                .With(l => l.Training, new List<TrainingDto>
                    {
                        _fixture
                            .Build<TrainingDto>()
                            .With(p => p.Reference, "ZPROG001")
                            .With(p => p.PriceEpisodes, new List<PriceEpisodeDto>()
                                {
                                    _fixture.Build<PriceEpisodeDto>()
                                        .With(x => x.AcademicYear,"2021")
                                        .With(pe => pe.Periods, new List<PeriodDto>()
                                        {
                                            _fixture.Build<PeriodDto>()
                                                .With(period => period.ApprenticeshipId,
                                                    _apprenticeshipIncentive2.ApprenticeshipId -
                                                    1) // ApprenticeshipId change
                                                .With(period => period.IsPayable, true)
                                                .With(period => period.Period, _periodNumber)
                                                .Create()
                                        })
                                        .With(pe => pe.StartDate, _startDate)
                                        .With(pe => pe.EndDate, (DateTime?) null)
                                        .Create()
                                }
                            )
                            .Create()
                    }
                )
                .Create();

            SetupMockLearnerMatchResponse(learner2Data);
        }

        [When(@"an exception occurs for a learner")]
        public async Task WhenAnExceptionOccursForALearner()
        {
            await StartLearnerMatching();
        }

        [Then(@"a record of learner match failure is created for the learner")]
        public void ThenARecordOfLearnerMatchFailureIsCreatedForTheLearner()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var learner = dbConnection.GetAll<Learner>().Single(l => l.ApprenticeshipIncentiveId == _apprenticeshipIncentive1.Id);
            learner.SuccessfulLearnerMatch.Should().BeFalse();
        }
        
        [Then(@"the learner match process is continued for all remaining learners")]
        public void ThenTheLearnerMatchProcessIsContinuedForAllRemainingLearners()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var learner = dbConnection.GetAll<Learner>().Single(l => l.ApprenticeshipIncentiveId == _apprenticeshipIncentive2.Id);
            learner.UpdatedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
        }
        
        [Then(@"a record of learner match success is created for all remaining learners")]
        public void ThenARecordOfLearnerMatchSuccessIsCreatedForAllRemainingLearners()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var learner = dbConnection.GetAll<Learner>().Single(l => l.ApprenticeshipIncentiveId == _apprenticeshipIncentive2.Id);
            learner.SuccessfulLearnerMatch.Should().BeTrue();
        }

        private void SetupMockLearnerMatchResponse(LearnerSubmissionDto learnerMatchApiData)
        {
            _testContext.LearnerMatchApi.MockServer
                .Given(
                    Request
                        .Create()
                        .WithPath($"/api/v1.0/{learnerMatchApiData.Ukprn}/{learnerMatchApiData.Uln}")
                        .UsingGet()
                )
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(learnerMatchApiData));
        }

        private void SetupMockLearnerMatchErrorResponse(long ukprn, long uln)
        {
            _testContext.LearnerMatchApi.MockServer
                .Given(
                    Request
                        .Create()
                        .WithPath($"/api/v1.0/{ukprn}/{uln}")
                        .UsingGet()
                )
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.BadRequest));
        }

        private async Task StartLearnerMatching()
        {
            var job = new OrchestrationStarterInfo(
                "LearnerMatchingOrchestrator_Start",
                nameof(LearnerMatchingOrchestrator),
                new Dictionary<string, object>
                {
                    ["req"] = new DummyHttpRequest
                    {
                        Path = "/api/orchestrators/LearnerMatchingOrchestrator"
                    }
                }
            );

            await _testContext.TestFunction.Start(job, false);
        }
    }
}
