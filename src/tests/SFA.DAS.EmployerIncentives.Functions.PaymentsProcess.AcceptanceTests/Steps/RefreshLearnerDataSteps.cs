using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Files;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "RefreshLearnerData")]
    public class RefreshLearnerDataSteps
    {
        private readonly TestContext _testContext;
        private readonly Account _accountModel;
        private readonly Fixture _fixture;
        private readonly ApprenticeshipIncentive _apprenticeshipIncentive;
        private readonly DateTime _startDate;
        private readonly DateTime _submissionDate;
        private readonly IList<PendingPayment> _pendingPayments;

        public RefreshLearnerDataSteps(TestContext testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();
            _startDate = DateTime.Parse("2020-08-10");
            _submissionDate = DateTime.Parse("2020-11-09T16:53:17.293+00:00");
            _accountModel = _fixture.Create<Account>();

            _apprenticeshipIncentive = _fixture.Build<ApprenticeshipIncentive>()
                .With(p => p.UKPRN, 10036143)
                .With(p => p.ULN, 9900084607)
                .With(p => p.ApprenticeshipId, 511526)
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
                .With(p => p.ActualStartDate, _startDate)
                .Create();

            _pendingPayments = new List<PendingPayment>
            {
                _fixture.Build<PendingPayment>()
                    .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                    .With(p => p.AccountId, _apprenticeshipIncentive.AccountId)
                    .With(p => p.AccountLegalEntityId, _apprenticeshipIncentive.AccountLegalEntityId)
                    .With(p => p.PeriodNumber, (byte?) 2) // previous period
                    .With(p => p.PaymentYear, (short?) 2020)
                    .With(p => p.PaymentMadeDate, DateTime.Now.AddDays(-1))
                    .Create(),
                _fixture.Build<PendingPayment>()
                    .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                    .With(p => p.AccountId, _apprenticeshipIncentive.AccountId)
                    .With(p => p.AccountLegalEntityId, _apprenticeshipIncentive.AccountLegalEntityId)
                    .With(p => p.PeriodNumber, (byte?) 3) // current period
                    .With(p => p.PaymentYear, (short?) 2020)
                    .With(p => p.DueDate, DateTime.Parse("2020-08-10"))
                    .Without(p => p.PaymentMadeDate)
                    .Create(),
                _fixture.Build<PendingPayment>()
                    .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                    .With(p => p.AccountId, _apprenticeshipIncentive.AccountId)
                    .With(p => p.AccountLegalEntityId, _apprenticeshipIncentive.AccountLegalEntityId)
                    .With(p => p.PeriodNumber, (byte?) 4) // future period
                    .With(p => p.PaymentYear, (short?) 2020)
                    .With(p => p.DueDate, DateTime.Parse("2020-09-10"))
                    .Without(p => p.PaymentMadeDate)
                    .Create()
            };
        }

        [Given(@"an apprenticeship incentive exists")]
        public async Task GivenAnApprenticeshipIncentiveExists()
        {
            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(_accountModel);
            await dbConnection.InsertAsync(_apprenticeshipIncentive);
            foreach (var pendingPayment in _pendingPayments)
            {
                await dbConnection.InsertAsync(pendingPayment);
            }
        }

        [Given(@"an apprenticeship incentive exists and without a corresponding learner match record")]
        public async Task GivenAnApprenticeshipIncentiveExistsWithoutACorrespondingLearnerMatchRecord()
        {
            await GivenAnApprenticeshipIncentiveExists();

            _testContext.LearnerMatchApi.MockServer
            .Given(
                    Request
                    .Create()
                    .WithPath($"/api/v1.0/{_apprenticeshipIncentive.UKPRN}/{_apprenticeshipIncentive.ULN}")
                    .UsingGet()
                    )
                .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.NotFound)
                .WithHeader("Content-Type", "application/json"));
        }

        [Given(@"an apprenticeship incentive exists and with a corresponding learner match record")]
        public async Task GivenAnApprenticeshipIncentiveExistsWithACorrespondingLearnerMatchRecord()
        {
            await GivenAnApprenticeshipIncentiveExists();
        }

        [Given(@"an apprenticeship incentive exists and has previously been refreshed")]
        public async Task GivenAnApprenticeshipIncentiveExistsAndHasPreviouslyBeenRefreshed()
        {
            await GivenAnApprenticeshipIncentiveExists();

            var learner = _testContext.TestData.GetOrCreate("ExistingLearner", onCreate: () =>
            {
                return _fixture.Build<Learner>()
                .With(s => s.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(s => s.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                .With(s => s.Ukprn, _apprenticeshipIncentive.UKPRN)
                .With(s => s.ULN, _apprenticeshipIncentive.ULN)
                .With(s => s.SubmissionFound, false)
                .Create();
            });

            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(learner);
        }

        [Given(@"the latest learner data has active in learning data")]
        public void AndTheLatestLearnerDataHasInLearningData()
        {
            SetupLearnerMatchApiResponse(LearnerMatchApiResponses.BL_R03_InLearning_json);
        }

        [Given(@"the latest learner data has a data locked price episode")]
        public void GivenLatestLearnerDataHasADataLockedPriceEpisode()
        {
            SetupLearnerMatchApiResponse(LearnerMatchApiResponses.Course_Price_Dlock_R03_json);
        }

        [Given(@"the latest learner data has training entries for a different apprenticeship")]
        public void GivenLatestLearnerDataHasNoPriceEpisodeForCurrentPeriod()
        {
            SetupLearnerMatchApiResponse(LearnerMatchApiResponses.BL_R03_InLearning_DifferentApprenticeship_json);
        }

        [Given(@"the latest learner data has no training entries")]
        public void GivenLatestLearnerDataHasNoTrainingEntries()
        {
            SetupLearnerMatchApiResponse(LearnerMatchApiResponses.BL_R03_InLearning_NoTraining);
        }

        [Given(@"the latest learner data has no ZPROG001 training entries")]
        public void GivenLatestLearnerDataHasNoZPROG001TrainingEntries()
        {
            SetupLearnerMatchApiResponse(LearnerMatchApiResponses.BL_R03_InLearning_NoZPROG001_json);
        }

        [When(@"the learner data is refreshed for the apprenticeship incentive")]
        public async Task WhenTheLearnerDataIsRefreshedForTheApprenticeshipIncentive()
        {
            await _testContext.PaymentsProcessFunctions.StartLearnerMatching();
        }

        [Then(@"the apprenticeship incentive learner data is created for the application without any submission data")]
        public void ThenTheApprenticeshipIncentiveLearnerDataIsCreatedForTheApplicationWithOutAnySubmissionData()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var createdLearners = dbConnection.GetAll<Learner>();

            var createdLearner = createdLearners.Single();

            createdLearner.SubmissionFound.Should().Be(false);
            createdLearner.Id.Should().NotBeEmpty();
            createdLearner.Ukprn.Should().Be(_apprenticeshipIncentive.UKPRN);
            createdLearner.ULN.Should().Be(_apprenticeshipIncentive.ULN);
            createdLearner.ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            createdLearner.ApprenticeshipId.Should().Be(_apprenticeshipIncentive.ApprenticeshipId);

            createdLearner.StartDate.Should().BeNull();
            createdLearner.DaysInLearning.Should().BeNull();
            createdLearner.SubmissionDate.Should().BeNull();
            createdLearner.RawJSON.Should().BeNull();
            createdLearner.InLearning.Should().BeNull();
            createdLearner.LearningFound.Should().BeNull();

            createdLearner.HasDataLock.Should().BeNull();
        }

        [Then(@"the apprenticeship incentive learner data is created for the application with submission data")]
        public void ThenTheApprenticeshipIncentiveLearnerDataIsCreatedForTheApplicationWithSubmissionData()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var createdLearners = dbConnection.GetAll<Learner>();

            var createdLearner = createdLearners.Single();

            createdLearner.SubmissionFound.Should().Be(true);
            createdLearner.Id.Should().NotBeEmpty();
            createdLearner.Ukprn.Should().Be(_apprenticeshipIncentive.UKPRN);
            createdLearner.ULN.Should().Be(_apprenticeshipIncentive.ULN);
            createdLearner.ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            createdLearner.ApprenticeshipId.Should().Be(_apprenticeshipIncentive.ApprenticeshipId);
            createdLearner.SubmissionDate.Should().Be(_submissionDate);
            createdLearner.RawJSON.Should().Be(LearnerMatchApiResponses.BL_R03_InLearning_json);
            createdLearner.StartDate.Should().Be(_startDate);

            createdLearner.DaysInLearning.Should().BeNull();
            createdLearner.InLearning.Should().BeTrue();
            createdLearner.HasDataLock.Should().BeFalse();
            createdLearner.LearningFound.Should().BeTrue();
        }

        [Then(@"the apprenticeship incentive learner data is updated for the application with submission data")]
        public void ThenTheApprenticeshipIncentiveLearnerDataIsUpdatedForTheApplicationWithSubmissionData()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var createdLearners = dbConnection.GetAll<Learner>();

            var createdLearner = createdLearners.Single(x => x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id);

            var testLearner = _testContext.TestData.Get<Learner>("ExistingLearner");

            createdLearner.Id.Should().Be(testLearner.Id);
            createdLearner.SubmissionFound.Should().Be(true);
            createdLearner.Id.Should().NotBeEmpty();
            createdLearner.Ukprn.Should().Be(_apprenticeshipIncentive.UKPRN);
            createdLearner.ULN.Should().Be(_apprenticeshipIncentive.ULN);
            createdLearner.ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            createdLearner.ApprenticeshipId.Should().Be(_apprenticeshipIncentive.ApprenticeshipId);
            createdLearner.SubmissionDate.Should().Be(_submissionDate);
            createdLearner.RawJSON.Should().Be(LearnerMatchApiResponses.BL_R03_InLearning_json);
            createdLearner.StartDate.Should().Be(_startDate);

            createdLearner.DaysInLearning.Should().BeNull();
            createdLearner.HasDataLock.Should().BeFalse();
            createdLearner.LearningFound.Should().BeTrue();
        }

        [When(@"the locked price episode period matches the next pending payment period")]
        public void TheLockedPriceEpisodePeriodMatchesTheNextPendingPaymentPeriod()
        {
            const byte lockedPeriod = 3; // see Course-Price-Dlock-R03.json.txt
            var nextPaymentPeriod = _pendingPayments.Where(x => x.PaymentMadeDate == null)
                .OrderBy(x => x.DueDate).First().PeriodNumber;
            nextPaymentPeriod.Should().Be(lockedPeriod);
        }

        [Then(@"the apprenticeship incentive learner data is updated indicating data lock")]
        public void ThenTheApprenticeshipIncentiveLearnerDataIsUpdatedIndicatingDataLock()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var createdLearner = dbConnection.GetAll<Learner>().Single(x => x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id);

            createdLearner.HasDataLock.Should().BeTrue();

            createdLearner.SubmissionFound.Should().Be(true);
            createdLearner.Id.Should().NotBeEmpty();
            createdLearner.Ukprn.Should().Be(_apprenticeshipIncentive.UKPRN);
            createdLearner.ULN.Should().Be(_apprenticeshipIncentive.ULN);
            createdLearner.ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            createdLearner.ApprenticeshipId.Should().Be(_apprenticeshipIncentive.ApprenticeshipId);
            createdLearner.SubmissionDate.Should().Be(DateTime.Parse("2020-11-09 17:04:31.407", new CultureInfo("en-GB")));
            createdLearner.RawJSON.Should().Be(LearnerMatchApiResponses.Course_Price_Dlock_R03_json);
            createdLearner.StartDate.Should().Be(_startDate);
            createdLearner.DaysInLearning.Should().BeNull();
        }

        [Then(@"the apprenticeship incentive learner data is updated indicating learning not found")]
        public void ThenTheApprenticeshipIncentiveLearnerDataIsUpdatedIndicatingLearningNotFound()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var createdLearner = dbConnection.GetAll<Learner>().Single(x => x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id);

            createdLearner.SubmissionFound.Should().Be(true);
            createdLearner.LearningFound.Should().BeFalse();
        }

        private void SetupLearnerMatchApiResponse(string json)
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
                    .WithBody(json));
        }
    }
}

