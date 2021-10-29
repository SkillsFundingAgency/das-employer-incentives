using System;
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

        [When(@"They have selected the apprenticeships for the application within the (.*) eligibility window")]
        public async Task WhenTheyHaveSelectedTheApprenticeshipsForTheApplication(string phase)
        {
            var employmentStartDate = new DateTime(2021, 10, 10);
            
            foreach (var apprentice in _request.Apprenticeships)
            {
                apprentice.EmploymentStartDate = employmentStartDate;
            }

            var url = $"applications";
            _response = await EmployerIncentiveApi.Post(url, _request);
        }

        [Then(@"the application is saved with the apprentices phases set to (.*)")]
        public async Task ThenTheApplicationIsSaved(string phase)
        {
            _response.StatusCode.Should().Be(_expectedResult);

            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var application = await dbConnection.QueryAsync<IncentiveApplication>("SELECT * FROM IncentiveApplication WHERE Id = @IncentiveApplicationId",
                    new { _request.IncentiveApplicationId });

                application.Should().HaveCount(1);
                application.Single().Id.Should().Be(_request.IncentiveApplicationId);

                var apprenticeships = await dbConnection.QueryAsync<IncentiveApplicationApprenticeship>("SELECT * FROM IncentiveApplicationApprenticeship WHERE IncentiveApplicationId = @IncentiveApplicationId",
                    new { _request.IncentiveApplicationId });

                apprenticeships.ToList().ForEach(a => a.Phase.ToString().Should().Be(phase));
            }

        }
    }
}
