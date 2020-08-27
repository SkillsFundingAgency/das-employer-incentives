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
using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Messages.Events;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "IncentiveApplicationSubmitted")]
    public class IncentiveApplicationSubmittedSteps : StepsBase
    {
        private readonly TestContext _testContext;
        private Fixture _fixture;
        private CreateIncentiveApplicationRequest _createRequest;
        private SubmitIncentiveApplicationRequest _submitRequest;

        public IncentiveApplicationSubmittedSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();
            _createRequest = _fixture.Create<CreateIncentiveApplicationRequest>();
            _submitRequest = _fixture.Create<SubmitIncentiveApplicationRequest>();
            _submitRequest.IncentiveApplicationId = _createRequest.IncentiveApplicationId;
            _submitRequest.AccountId = _createRequest.AccountId;
        }

        [Given(@"an employer has entered incentive claim application details")]
        public async Task GivenAnEmployerHasEnteredIncentiveClaimApplicationDetails()
        {
            var url = $"applications";
            await EmployerIncentiveApi.Post(url, _createRequest);
            EmployerIncentiveApi.Response.StatusCode.Should().Be(HttpStatusCode.Created);

            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var application = await dbConnection.QueryAsync<IncentiveApplication>("SELECT * FROM IncentiveApplication WHERE Id = @IncentiveApplicationId",
                    new { _submitRequest.IncentiveApplicationId });

                application.Should().HaveCount(1);
                application.Single().Id.Should().Be(_submitRequest.IncentiveApplicationId);
            }
        }

        [When(@"the application is submitted")]
        public async Task WhenTheApplicationIsSubmitted()
        {
            var url = $"applications/{_submitRequest.IncentiveApplicationId}";
            await EmployerIncentiveApi.Patch(url, _submitRequest);
        }

        [Then(@"the application status is updated to reflect completion")]
        public async Task ThenTheApplicationStatusIsUpdatedToReflectCompletion()
        {
            EmployerIncentiveApi.Response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var application = await dbConnection.QueryAsync<IncentiveApplication>("SELECT * FROM IncentiveApplication WHERE Id = @IncentiveApplicationId AND Status = 'Submitted'",
                    new { _submitRequest.IncentiveApplicationId });

                application.Should().HaveCount(1);
                application.Single().Id.Should().Be(_submitRequest.IncentiveApplicationId);
            }

            var publishedEvents = _testContext.TestData.Get<List<EmployerIncentiveClaimSubmittedEvent>>();

            var publishedEvent = publishedEvents.SingleOrDefault(e => 
            e.AccountId == _submitRequest.AccountId &&
            e.IncentiveClaimApplicationId == _submitRequest.IncentiveApplicationId);

            publishedEvent.Should().NotBeNull();
        }

        [When(@"the invalid application id is submittted")]
        public async Task WhenTheInvalidApplicationIdIsSubmittted()
        {
            var invalidApplicationId = _fixture.Create<Guid>();
            _submitRequest.IncentiveApplicationId = invalidApplicationId;
            var url = $"applications/{_submitRequest.IncentiveApplicationId}";
            await EmployerIncentiveApi.Patch(url, _submitRequest);
        }

        [Then(@"the application status is not updated")]
        public async Task ThenTheApplicationStatusIsNotUpdated()
        {
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var application = await dbConnection.QueryAsync<IncentiveApplication>("SELECT * FROM IncentiveApplication WHERE Id = @IncentiveApplicationId AND Status = 'Submitted'",
                    new { _submitRequest.IncentiveApplicationId });

                application.Should().HaveCount(0);
            }
        }

        [Then(@"the service responds with an error")]
        public void ThenTheServiceRespondsWithAnError()
        {
            EmployerIncentiveApi.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [When(@"the invalid account id is submittted")]
        public async Task WhenTheInvalidAccountIdIsSubmittted()
        {
            var invalidAccountId = _fixture.Create<long>();
            _submitRequest.AccountId = invalidAccountId;
            var url = $"applications/{_submitRequest.IncentiveApplicationId}";
            await EmployerIncentiveApi.Patch(url, _submitRequest);
        }

    }
}
