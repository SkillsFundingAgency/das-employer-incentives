using System.Data.SqlClient;
using System.Linq;
using FluentAssertions;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using Dapper;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Data.Models;
using TechTalk.SpecFlow;
using System.Net.Http;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "IncentiveApplicationCreated")]
    public class IncentiveApplicationCreatedSteps : StepsBase
    {
        private readonly TestContext _testContext;
        private readonly Fixture _fixture;
        private readonly HttpStatusCode _expectedResult = HttpStatusCode.Created;
        private readonly CreateIncentiveApplicationRequest _request;
        private HttpResponseMessage _response;

        public IncentiveApplicationCreatedSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();
            _request = _fixture.Create<CreateIncentiveApplicationRequest>();
        }

        [Given(@"An employer is applying for the New Apprenticeship Incentive")]
        public void GivenAnEmployerIsApplyingForTheNewApprenticeshipIncentive()
        {
            // 
        }

        [When(@"They have selected the apprenticeships for the application")]
        public async Task WhenTheyHaveSelectedTheApprenticeshipsForTheApplication()
        {
            var url = $"applications";
            _response = await EmployerIncentiveApi.Post(url, _request);
        }

        [Then(@"the application is saved")]
        public async Task ThenTheApplicationIsSaved()
        {
            _response.StatusCode.Should().Be(_expectedResult);

            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var application = await dbConnection.QueryAsync<IncentiveApplication>("SELECT * FROM IncentiveApplication WHERE Id = @IncentiveApplicationId",
                    new { _request.IncentiveApplicationId });

                application.Should().HaveCount(1);
                application.Single().Id.Should().Be(_request.IncentiveApplicationId);
            }
        }
    }
}
