﻿using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
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
    public class RefreshLearnerDataSteps //: StepsBase
    {
        private readonly TestContext _testContext;
        private readonly Account _accountModel;
        private readonly Fixture _fixture;
        private readonly ApprenticeshipIncentive _apprenticeshipIncentive;
        private readonly LearnerSubmissionDto _learnerMatchApiData;

        public RefreshLearnerDataSteps(TestContext testContext) //: base(testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();

            _accountModel = _fixture.Create<Account>();

            _apprenticeshipIncentive = _fixture.Build<ApprenticeshipIncentive>()
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
                .Create();

            _learnerMatchApiData = _fixture.Build<LearnerSubmissionDto>()
                .With(s => s.Ukprn, _apprenticeshipIncentive.UKPRN)
                .With(s => s.Learner, _fixture.Build<LearnerDto>().With(l => l.Uln, _apprenticeshipIncentive.Uln).Create())
                .Create();
        }

        public async Task GivenAnApprenticeshipIncentiveExists()
        {
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.InsertAsync(_accountModel);
                await dbConnection.InsertAsync(_apprenticeshipIncentive);
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
                .WithBodyAsJson(_learnerMatchApiData));
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

            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
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
                .WithBodyAsJson(_learnerMatchApiData));
        }

        [When(@"the learner data is refreshed for the apprenticeship incentive")]
        public async Task WhenTheLearnerDataIsRefreshedForTheApprenticeshipIncentive()
        {

            await _testContext.PaymentsProcessFunctions.StartLearnerMatching();

            //await EmployerIncentiveApi.PostCommand(
            //        $"commands/ApprenticeshipIncentive.RefreshLearnerCommand",
            //        new RefreshLearnerCommand(_apprenticeshipIncentive.Id));

            //EmployerIncentiveApi.Response.StatusCode.Should().Be(HttpStatusCode.OK);

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

            createdLearner.DaysInLearning.Should().BeNull();
            createdLearner.HasDataLock.Should().BeNull();
            createdLearner.SubmissionDate.Should().BeNull();
            createdLearner.DaysInLearning.Should().BeNull();
            createdLearner.RawJSON.Should().BeNull();
            createdLearner.StartDate.Should().BeNull();
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
            createdLearner.SubmissionDate.Should().Be(_learnerMatchApiData.IlrSubmissionDate);
            createdLearner.RawJSON.Should().Be(JsonConvert.SerializeObject(_learnerMatchApiData));

            createdLearner.DaysInLearning.Should().BeNull();
            createdLearner.HasDataLock.Should().BeNull();
            createdLearner.DaysInLearning.Should().BeNull();
            createdLearner.StartDate.Should().BeNull();
        }

        [Then(@"the apprenticeship incentive learner data is updated for the application with submission data")]
        public void ThenTheApprenticeshipIncentiveLearnerDataIsUpdatedForTheApplicationWithSubmissionData()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var createdLearners = dbConnection.GetAll<Learner>();

            var createdLearner = createdLearners.Single();

            var testLearner = _testContext.TestData.Get<Learner>("ExistingLearner");

            createdLearner.Id.Should().Be(testLearner.Id);
            createdLearner.SubmissionFound.Should().Be(true);
            createdLearner.Id.Should().NotBeEmpty();
            createdLearner.Ukprn.Should().Be(_apprenticeshipIncentive.UKPRN);
            createdLearner.ULN.Should().Be(_apprenticeshipIncentive.Uln);
            createdLearner.ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            createdLearner.ApprenticeshipId.Should().Be(_apprenticeshipIncentive.ApprenticeshipId);
            createdLearner.SubmissionDate.Should().Be(_learnerMatchApiData.IlrSubmissionDate);
            createdLearner.RawJSON.Should().Be(JsonConvert.SerializeObject(_learnerMatchApiData));

            createdLearner.DaysInLearning.Should().BeNull();
            createdLearner.HasDataLock.Should().BeNull();
            createdLearner.DaysInLearning.Should().BeNull();
            createdLearner.StartDate.Should().BeNull();
        }
    }
}

