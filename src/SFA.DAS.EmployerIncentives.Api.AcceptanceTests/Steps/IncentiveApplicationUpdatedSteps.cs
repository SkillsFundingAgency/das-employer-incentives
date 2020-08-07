using AutoFixture;
using CSScriptLib;
using Dapper;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Data.Models;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "IncentiveApplicationUpdated")]
    public class IncentiveApplicationUpdatedSteps : StepsBase
    {
        private readonly TestContext _testContext;
        private UpdateIncentiveApplicationRequest _updateRequest;

        public IncentiveApplicationUpdatedSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
        }

        [Given(@"An employer is applying for the New Apprenticeship Incentive")]
        public async Task GivenAnEmployerIsApplyingForTheNewApprenticeshipIncentive()
        {
            var createRequest = Fixture.Create<CreateIncentiveApplicationRequest>();
            const string url = "applications";
            await EmployerIncentiveApi.Post(url, createRequest);

            _updateRequest = new UpdateIncentiveApplicationRequest()
            {
                IncentiveApplicationId = createRequest.IncentiveApplicationId,
                Apprenticeships = Fixture.CreateMany<IncentiveApplicationApprenticeshipDto>(4),
                AccountId = createRequest.AccountId,
                AccountLegalEntityId = createRequest.AccountLegalEntityId
            };
            _updateRequest.Apprenticeships.AddItem(createRequest.Apprenticeships.First());
        }

        [When(@"They have changed selected apprenticeships for the application")]
        public async Task WhenTheyHaveChangedSelectedApprenticeshipsForTheApplication()
        {
            var url = $"applications/{_updateRequest.IncentiveApplicationId}";
            await EmployerIncentiveApi.Put(url, _updateRequest);
        }

        [Then(@"the application is updated with new selection of apprenticeships")]
        public void ThenTheApplicationIsUpdatedWithNewSelectionOfApprenticeships()
        {
            EmployerIncentiveApi.Response.StatusCode.Should().Be(HttpStatusCode.OK);

            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);

            var query = $"SELECT * FROM IncentiveApplicationApprenticeship WHERE IncentiveApplicationId = '{ _updateRequest.IncentiveApplicationId}'";
            var apprenticeships = dbConnection.Query<IncentiveApplicationApprenticeship>(query).ToList();

            apprenticeships.Should().BeEquivalentTo(_updateRequest.Apprenticeships);
        }

    }
}
