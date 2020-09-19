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
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "ApprenticeshipIncentiveCreated")]
    public class ApprenticeshipIncentiveCreatedSteps : StepsBase
    {
        private readonly TestContext _testContext;
        private readonly IncentiveApplication _applicationModel;
        private readonly Fixture _fixture;
        private readonly List<IncentiveApplicationApprenticeship> _apprenticeshipsModels;
        private const int NumberOfApprenticeships = 3;

        public ApprenticeshipIncentiveCreatedSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();

            _applicationModel = _fixture.Build<IncentiveApplication>()
                .With(p => p.Status, IncentiveApplicationStatus.InProgress)
                .Create();

            _apprenticeshipsModels = _fixture.Build<IncentiveApplicationApprenticeship>()
                .With(p => p.IncentiveApplicationId, _applicationModel.Id)
                .With(p => p.PlannedStartDate, DateTime.Today.AddDays(1))
                .With(p => p.DateOfBirth, DateTime.Today.AddYears(-20))
                .With(p => p.EarningsCalculated, false)
                .CreateMany(NumberOfApprenticeships).ToList();
        }

        [Given(@"an employer is applying for the New Apprenticeship Incentive")]
        public async Task GivenAnEmployerIsApplyingForTheNewApprenticeshipIncentive()
        {
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.InsertAsync(_applicationModel);
                await dbConnection.InsertAsync(_apprenticeshipsModels);
            }
        }

        [Given(@"an employer has submitted an application")]
        public async Task GivenAnEmployerHasSubmittedAnApplication()
        {
            await GivenAnEmployerIsApplyingForTheNewApprenticeshipIncentive();
        }

        [When(@"they submit the application")]
        public async Task WhenTheySubmitTheApplication()
        {
            var submitRequest = _fixture.Build<SubmitIncentiveApplicationRequest>()
                .With(p => p.IncentiveApplicationId, _applicationModel.Id)
                .With(p => p.AccountId, _applicationModel.AccountId)
                .With(p => p.DateSubmitted, DateTime.Today)
                .Create();

            var url = $"applications/{_applicationModel.Id}";
            await EmployerIncentiveApi.Patch(url, submitRequest);
        }

        [When(@"the apprenticeship incentice is created for each apprenticship in the application")]
        public async Task WhenTheApprenticeshipIncentiveIsCreatedForEachApprenticeshipInTheApplication()
        {
            var createCommand = new CreateCommand(_applicationModel.AccountId, _applicationModel.Id);
       
            await EmployerIncentiveApi.PostCommand(
                    $"commands/ApprenticeshipIncentive.CreateCommand",
                    createCommand);

            EmployerIncentiveApi.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Then(@"the apprentiveship incentive is created for the application")]
        public void ThenTheApprenticeshipIncentiveIsCreatedForTheApplication()
        {
            _testContext.CommandsPublished.Single(c => c.IsPublished).Command.Should().BeOfType<CreateCommand>();
        }

        [Then(@"the earnings are calculated for each apprenticeship incentice")]
        public void ThenTheEarningsAreCalculatedForEachApprenticeshipIncentive()
        {
            var commandsPublished = _testContext.CommandsPublished.Where(c => c.IsPublished && c.Command.GetType() == typeof(CalculateEarningsCommand));

            commandsPublished.Count().Should().Be(NumberOfApprenticeships);

            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var createdIncentives = dbConnection.GetAll<ApprenticeshipIncentive>();

                createdIncentives.Count().Should().Be(NumberOfApprenticeships);
            }
        }
    }
}
