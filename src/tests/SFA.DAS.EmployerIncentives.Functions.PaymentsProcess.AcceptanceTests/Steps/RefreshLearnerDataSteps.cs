﻿using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Files;
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
                .With(p => p.Uln, 9900084607)
                .With(p => p.ApprenticeshipId, 511526)
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
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
                    .WithPath($"/api/v1.0/{_apprenticeshipIncentive.UKPRN}/{_apprenticeshipIncentive.Uln}")
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

            _testContext.LearnerMatchApi.MockServer
            .Given(
                    Request
                    .Create()
                    .WithPath($"/api/v1.0/{_apprenticeshipIncentive.UKPRN}/{_apprenticeshipIncentive.Uln}")
                    .UsingGet()
                    )
                .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(LearnerMatchApiResponses.BL_R03_InLearning_json));
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
                .With(s => s.ULN, _apprenticeshipIncentive.Uln)
                .With(s => s.SubmissionFound, false)
                .Create();
            });

            await using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.InsertAsync(learner);
            }

            _testContext.LearnerMatchApi.MockServer
            .Given(
                    Request
                    .Create()
                    .WithPath($"/api/v1.0/{_apprenticeshipIncentive.UKPRN}/{_apprenticeshipIncentive.Uln}")
                    .UsingGet()
                    )
                .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(LearnerMatchApiResponses.BL_R03_InLearning_json));
        }

        [Given(@"an apprenticeship incentive exists with a data locked price episode")]
        public async Task GivenAnApprenticeshipIncentiveExistsWithADataLockedPriceEpisode()
        {
            await GivenAnApprenticeshipIncentiveExists();

            _testContext.LearnerMatchApi.MockServer
                .Given(
                    Request
                        .Create()
                        .WithPath($"/api/v1.0/{_apprenticeshipIncentive.UKPRN}/{_apprenticeshipIncentive.Uln}")
                        .UsingGet()
                )
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(LearnerMatchApiResponses.Course_Price_Dlock_R03_json));
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
            createdLearner.ULN.Should().Be(_apprenticeshipIncentive.Uln);
            createdLearner.ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            createdLearner.ApprenticeshipId.Should().Be(_apprenticeshipIncentive.ApprenticeshipId);

            createdLearner.StartDate.Should().BeNull();
            createdLearner.DaysInLearning.Should().BeNull();
            createdLearner.HasDataLock.Should().BeNull();
            createdLearner.SubmissionDate.Should().BeNull();
            createdLearner.RawJSON.Should().BeNull();
            createdLearner.InLearning.Should().BeNull();
            createdLearner.LearningFound.Should().BeFalse();
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
            createdLearner.ULN.Should().Be(_apprenticeshipIncentive.Uln);
            createdLearner.ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            createdLearner.ApprenticeshipId.Should().Be(_apprenticeshipIncentive.ApprenticeshipId);
            createdLearner.SubmissionDate.Should().Be(_submissionDate);
            createdLearner.RawJSON.Should().Be(LearnerMatchApiResponses.BL_R03_InLearning_json);
            createdLearner.StartDate.Should().Be(_startDate);

            createdLearner.DaysInLearning.Should().BeNull();
            createdLearner.InLearning.Should().BeNull();
            createdLearner.HasDataLock.Should().BeNull();
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
            createdLearner.ULN.Should().Be(_apprenticeshipIncentive.Uln);
            createdLearner.ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            createdLearner.ApprenticeshipId.Should().Be(_apprenticeshipIncentive.ApprenticeshipId);
            createdLearner.SubmissionDate.Should().Be(_submissionDate);
            createdLearner.RawJSON.Should().Be(LearnerMatchApiResponses.BL_R03_InLearning_json);
            createdLearner.StartDate.Should().Be(_startDate);

            createdLearner.DaysInLearning.Should().BeNull();
            createdLearner.HasDataLock.Should().BeNull();
            createdLearner.LearningFound.Should().BeTrue();
        }

        [When(@"the locked price episode period matches the next pending payment period")]
        public void TheLockedPriceEpisodePeriodMatchesTheNextPendingPaymentPeriod()
        {
            const byte lockedPeriod = 3; // see Course-Price-Dlock-R03.json.txt
            var nextPaymentPeriod = _pendingPayments.Where(x => x.PaymentMadeDate == null).OrderBy(x => x.DueDate).First().PeriodNumber;
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
            createdLearner.ULN.Should().Be(_apprenticeshipIncentive.Uln);
            createdLearner.ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            createdLearner.ApprenticeshipId.Should().Be(_apprenticeshipIncentive.ApprenticeshipId);
            createdLearner.SubmissionDate.Should().Be(_submissionDate);
            createdLearner.RawJSON.Should().Be(LearnerMatchApiResponses.BL_R04_InBreak_json);
            createdLearner.StartDate.Should().Be(_startDate);
            createdLearner.DaysInLearning.Should().BeNull();
        }

    }
}

