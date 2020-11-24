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
using TechTalk.SpecFlow;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "IlrStartDateChanged")]
    public class IlrStartDateChangedSteps
    {
        private readonly TestContext _testContext;
        private readonly Account _accountModel;
        private readonly Fixture _fixture;
        private readonly ApprenticeshipIncentive _apprenticeshipIncentive;
        private readonly PendingPayment _pendingPayment;
        private readonly LearnerSubmissionDto _learnerMatchApiData;
        private readonly DateTime _plannedStartDate;

        public IlrStartDateChangedSteps(TestContext testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();

            _plannedStartDate = _fixture.Create<DateTime>();
            _accountModel = _fixture.Create<Account>();

            _apprenticeshipIncentive = _fixture.Build<ApprenticeshipIncentive>()
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
                .Create();

            _pendingPayment = _fixture.Build<PendingPayment>()
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.DueDate, _plannedStartDate.AddMonths(1))
                .Create();

            _pendingPayment.PaymentMadeDate = null;

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

        [Given(@"an apprenticeship incentive exists")]
        public async Task GivenAnApprenticeshipIncentiveExists()
        {
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.InsertAsync(_accountModel);
                await dbConnection.InsertAsync(_apprenticeshipIncentive);
                await dbConnection.InsertAsync(_pendingPayment);
            }
        }

        [When(@"the learner data is refreshed with a new valid start date for the apprenticeship incentive")]
        public async Task WhenTheLearnerIsRefreshedWithAValidStartDate()
        {
            var actualStartDate = new DateTime(2020, 9, 1);
            _learnerMatchApiData.Training.First().PriceEpisodes.First().StartDate = actualStartDate;
            _learnerMatchApiData.Training.First().PriceEpisodes.First().EndDate = actualStartDate.AddYears(1);

            SetupMockLearnerMatchResponse();

            await _testContext.PaymentsProcessFunctions.StartLearnerMatching();
        }

        [When(@"the learner data is refreshed with a new invalid start date for the apprenticeship incentive")]
        public async Task WhenTheLearnerIsRefreshedWithAnInvalidStartDate()
        {
            var actualStartDate = new DateTime(2020, 7, 1);
            _learnerMatchApiData.Training.First().PriceEpisodes.First().StartDate = actualStartDate;
            _learnerMatchApiData.Training.First().PriceEpisodes.First().EndDate = actualStartDate.AddYears(1);

            SetupMockLearnerMatchResponse();

            await _testContext.PaymentsProcessFunctions.StartLearnerMatching();
        }

        [Then(@"the actual start date is updated")]
        public void ThenActualStartDateIsUpdated()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var incentive = dbConnection.GetAll<ApprenticeshipIncentive>();

            incentive.Single().ActualStartDate.Should().Be(_learnerMatchApiData.Training.First().PriceEpisodes.First().StartDate);
        }

        [Then(@"the pending payments are recalculated for the apprenticeship incentive")]
        public void ThenPendingPaymentsAreRecalculated()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var pendingPayments = dbConnection.GetAll<PendingPayment>();

            pendingPayments.Count().Should().Be(2);
        }

        [Then(@"the learner data is subsequently refreshed")]
        public void ThenLearnerRefreshIsCalledAgain()
        {
            _testContext.LearnerMatchApi.MockServer.LogEntries.Count(x =>
                    x.RequestMessage.Path ==  $"/api/v1.0/{_apprenticeshipIncentive.UKPRN}/{_apprenticeshipIncentive.ULN}")
                .Should().Be(2);
        }

        [Then(@"the existing pending payments are removed")]
        public void ThenPendingPaymentsAreRemoved()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var pendingPayments = dbConnection.GetAll<PendingPayment>();

            pendingPayments.Should().BeEmpty();
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
    }
}
