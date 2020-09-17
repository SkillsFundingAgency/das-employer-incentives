using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using NServiceBus.Transport;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Hooks;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
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

        [Then(@"the apprentiveship incentive is created for the application")]
        public void ThenTheApprenticeshipIncentiveIsCreatedForTheApplication()
        {
            _testContext.CommandsPublished.Single(c => c.IsPublished).Command.Should().BeOfType<CreateCommand>();
        }
    }
}
