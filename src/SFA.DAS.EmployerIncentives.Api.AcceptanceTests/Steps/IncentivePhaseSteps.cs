using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "IncentivePhase")]
    public class IncentivePhaseSteps : StepsBase
    {
        private readonly TestContext _testContext;
        private readonly Fixture _fixture;
        private SubmitIncentiveApplicationRequest _submitRequest;
        private HttpResponseMessage _response;
        private readonly Account _accountModel;
        private readonly IncentiveApplication _applicationModel;
        private readonly IncentiveApplicationApprenticeship _incentiveApplicationApprenticeshipModel;
        private readonly List<IncentiveApplicationApprenticeship> _apprenticeshipsModels;

        public IncentivePhaseSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();
            var today = new DateTime(2021, 1, 30);

            _accountModel = _fixture.Create<Account>();
            _applicationModel = _fixture.Build<IncentiveApplication>()
                .With(p => p.Status, IncentiveApplicationStatus.InProgress)
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
                .Create();

            _incentiveApplicationApprenticeshipModel = _fixture.Build<IncentiveApplicationApprenticeship>()
                .With(p => p.IncentiveApplicationId, _applicationModel.Id)
                .With(p => p.EarningsCalculated, false)
                .With(p => p.WithdrawnByCompliance, false)
                .With(p => p.WithdrawnByEmployer, false)
                .With(p => p.DateOfBirth, today.AddYears(-20))
                .With(p => p.Phase, Phase.NotSet)
                .Create();

            _apprenticeshipsModels = new List<IncentiveApplicationApprenticeship>
            {
                _incentiveApplicationApprenticeshipModel
            };
        }

        [Given(@"an employer is applying for an apprenticeship with a start date of '(.*)'")]
        public async Task GivenAnEmployerIsApplyingForAnApprenticeshipWithAStartDateOf(string date)
        {
            var startDate = DateTime.ParseExact(date, "yyyy-MM-d", CultureInfo.InvariantCulture);

            _incentiveApplicationApprenticeshipModel.PlannedStartDate = startDate;            

            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.InsertAsync(_accountModel);
                await dbConnection.InsertAsync(_applicationModel);
                await dbConnection.InsertAsync(_apprenticeshipsModels);
            }
        }

        [When(@"they submit the application on '(.*)'")]
        public async Task WhenTheySubmitApplicationOn(string date)
        {
            var submissionDate = DateTime.ParseExact(date, "yyyy-MM-d", CultureInfo.InvariantCulture);
            
            _submitRequest = _fixture.Build<SubmitIncentiveApplicationRequest>()
                .With(r => r.DateSubmitted, submissionDate)
                .Create();

            _submitRequest.IncentiveApplicationId = _applicationModel.Id;
            _submitRequest.AccountId = _accountModel.Id;

            var url = $"applications/{_submitRequest.IncentiveApplicationId}";

            await _testContext.WaitFor(
                async (cancellationToken) =>
                {
                    _response = await EmployerIncentiveApi.Patch(url, _submitRequest);
                },
                (context) => HasExpectedEvents(context)
                );

            _response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        private bool HasExpectedEvents(TestContext testContext)
        {
            return testContext.CommandsPublished.Count(c => c.IsProcessed &&
                    c.IsDomainCommand &&
                    c.Command is CreateIncentiveCommand) == 1;
        }

        [Then(@"the apprenticeship incentive phase for the the application is '(.*)'")]
        public async Task ThenTheApprenticeshipIncentivePhaseForTheApplicationIs(string phaseString)
        {
            var expectedPhase = Enum.Parse<Phase>(phaseString);

            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var apprenticeApplications = connection.GetAllAsync<IncentiveApplicationApprenticeship>().Result.ToList();
            var apprenticeApplication = apprenticeApplications.Single();

            apprenticeApplication.IncentiveApplicationId.Should().Be(_applicationModel.Id);

            apprenticeApplication.Phase.Should().Be(expectedPhase);
        }

        [Then(@"the apprenticeship incentive is created with an incentive phase of '(.*)'")]
        public async Task ThenTheApprenticeshipIncentiveIsCreatedWithAnIncentivePhaseOf(string phaseString)
        {
            var expectedPhase = Enum.Parse<Phase>(phaseString);

            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var apprenticeIncentives = connection.GetAllAsync<ApprenticeshipIncentive>().Result.ToList();
            var apprenticeIncentive = apprenticeIncentives.Single();

            apprenticeIncentive.ApprenticeshipId.Should().Be(_incentiveApplicationApprenticeshipModel.ApprenticeshipId);

            apprenticeIncentive.Phase.Should().Be(expectedPhase);
        }
    }
}
