using AutoFixture;
using Dapper;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Data.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "IncentiveApplicationSubmitted")]
    public class IncentiveApplicationSubmittedSteps : StepsBase
    {
        private readonly TestContext _testContext;
        private readonly Fixture _fixture;
        private CreateIncentiveApplicationRequest _createRequest;
        private SubmitIncentiveApplicationRequest _submitRequest;
        private HttpResponseMessage _response;
        private long _firstApprenticeshipId;
        private long _secondApprenticeshipId;

        public IncentiveApplicationSubmittedSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();
            _firstApprenticeshipId = _fixture.Create<long>();
            _secondApprenticeshipId = _firstApprenticeshipId + 1;
            var apprenticeships = new List<IncentiveApplicationApprenticeshipDto>
            {
                _fixture.Build<IncentiveApplicationApprenticeshipDto>().With(p => p.ApprenticeshipId, _firstApprenticeshipId).With(p => p.PlannedStartDate, new DateTime(2021, 10, 1)).With(p => p.EmploymentStartDate, new DateTime(2021, 10, 01)).Create(),
                _fixture.Build<IncentiveApplicationApprenticeshipDto>().With(p => p.ApprenticeshipId, _secondApprenticeshipId).With(p => p.PlannedStartDate, new DateTime(2021, 12, 1)).With(p => p.EmploymentStartDate, new DateTime(2021, 11, 01)).Create()
            };

            _createRequest = _fixture
                .Build<CreateIncentiveApplicationRequest>()
                .With(r => r.Apprenticeships, apprenticeships)
                .Create();
            _submitRequest = _fixture.Build<SubmitIncentiveApplicationRequest>().With(r => r.DateSubmitted, new DateTime(2021, 5, 31)).Create();
            _submitRequest.IncentiveApplicationId = _createRequest.IncentiveApplicationId;
            _submitRequest.AccountId = _createRequest.AccountId;
        }

        [Given(@"an employer has entered incentive claim application details")]
        public async Task GivenAnEmployerHasEnteredIncentiveClaimApplicationDetails()
        {
            var url = $"applications";
            _response = await EmployerIncentiveApi.Post(url, _createRequest);
            _response.StatusCode.Should().Be(HttpStatusCode.Created);

            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var application = await dbConnection.QueryAsync<IncentiveApplication>("SELECT * FROM IncentiveApplication WHERE Id = @IncentiveApplicationId",
                    new { _submitRequest.IncentiveApplicationId });

                application.Should().HaveCount(1);
                application.Single().Id.Should().Be(_submitRequest.IncentiveApplicationId);
            }
        }

        [Given(@"an employer has entered incentive claim application details with employment start dates for Phase 3")]
        public async Task GivenAnEmployerHasEnteredIncentiveClaimApplicationDetailsForPhase3()
        {
            var apprenticeships = new List<IncentiveApplicationApprenticeshipDto>
            {
                _fixture.Build<IncentiveApplicationApprenticeshipDto>().With(p => p.ApprenticeshipId, _firstApprenticeshipId).With(p => p.PlannedStartDate, new DateTime(2021, 10, 1)).With(p => p.EmploymentStartDate, new DateTime(2021, 10, 01)).Create(),
                _fixture.Build<IncentiveApplicationApprenticeshipDto>().With(p => p.ApprenticeshipId, _secondApprenticeshipId).With(p => p.PlannedStartDate, new DateTime(2022, 1, 1)).With(p => p.EmploymentStartDate, new DateTime(2022, 01, 31)).Create()
            };

            _createRequest.Apprenticeships = apprenticeships;

            var url = $"applications";
            _response = await EmployerIncentiveApi.Post(url, _createRequest);
            _response.StatusCode.Should().Be(HttpStatusCode.Created);

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
            _response = await EmployerIncentiveApi.Patch(url, _submitRequest);
        }

        [When(@"the application is submitted and the system errors")]
        public async Task WhenTheApplicationIsSubmittedAndTheSystemErrors()
        {
            _testContext.TestData.Set("ThrowErrorAfterProcessedCommand", true);

            await WhenTheApplicationIsSubmitted();
        }

        [Then(@"the application status is updated to reflect completion")]
        public async Task ThenTheApplicationStatusIsUpdatedToReflectCompletion()
        {
            _response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var application = await dbConnection.QueryAsync<IncentiveApplication>("SELECT * FROM IncentiveApplication WHERE Id = @IncentiveApplicationId AND Status = 'Submitted'",
                    new { _submitRequest.IncentiveApplicationId });

                application.Should().HaveCount(1);
                application.Single().Id.Should().Be(_submitRequest.IncentiveApplicationId);

                var apprenticeships = await dbConnection.QueryAsync<IncentiveApplicationApprenticeship>("SELECT * FROM IncentiveApplicationApprenticeship WHERE IncentiveApplicationId = @IncentiveApplicationId",
                    new { _submitRequest.IncentiveApplicationId });

                apprenticeships.Count().Should().Be(2);
                apprenticeships.Single(a => a.ApprenticeshipId == _firstApprenticeshipId).Phase.Should().Be(Enums.Phase.Phase3);
                apprenticeships.Single(a => a.ApprenticeshipId == _secondApprenticeshipId).Phase.Should().Be(Enums.Phase.Phase3);
            }

            var publishedCommand = _testContext.CommandsPublished
                .Where(c => c.IsPublished && 
                c.IsDomainCommand &&
                c.Command is CreateIncentiveCommand)
                .Select(c => c.Command).ToArray();

            publishedCommand.Count().Should().Be(_createRequest.Apprenticeships.Count());

            var cmd = publishedCommand.First() as CreateIncentiveCommand;
            cmd.AccountId.Should().Be(_submitRequest.AccountId);
        }

        [Then(@"the application apprentice phases are set to Phase3")]
        public async Task ThenTheApplicationApprenticePhasesAreSetToPhase3()
        {
            _response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var apprenticeships = await dbConnection.QueryAsync<IncentiveApplicationApprenticeship>("SELECT * FROM IncentiveApplicationApprenticeship WHERE IncentiveApplicationId = @IncentiveApplicationId",
                    new { _submitRequest.IncentiveApplicationId });

                apprenticeships.Count().Should().Be(2);
                apprenticeships.Single(a => a.ApprenticeshipId == _firstApprenticeshipId).Phase.Should().Be(Enums.Phase.Phase3);
                apprenticeships.Single(a => a.ApprenticeshipId == _secondApprenticeshipId).Phase.Should().Be(Enums.Phase.Phase3);
            }
        }

        [When(@"the invalid application id is submitted")]
        public async Task WhenTheInvalidApplicationIdIsSubmitted()
        {
            var invalidApplicationId = _fixture.Create<Guid>();
            _submitRequest.IncentiveApplicationId = invalidApplicationId;
            var url = $"applications/{_submitRequest.IncentiveApplicationId}";
            _response = await EmployerIncentiveApi.Patch(url, _submitRequest);
        }

        [Then(@"the application changes are not saved")]
        public async Task ThenTheApplicationChangesAreNotSaved()
        {
            await ThenTheApplicationStatusIsNotUpdated();
            ThenTheServiceRespondsWithAnInternalError();
            await ThenThereAreNoEventsInTheOutbox();
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
            _response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Then(@"the service responds with an internal error")]
        public void ThenTheServiceRespondsWithAnInternalError()
        {
            _response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Then(@"there are no events in the outbox")]
        public async Task ThenThereAreNoEventsInTheOutbox()
        {
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var messages = (await dbConnection.QueryAsync<object>("SELECT 1 FROM ClientOutboxData"));
                var messagesExist = messages.Any();

                messagesExist.Should().BeFalse();
            }
        }

        [When(@"the invalid account id is submitted")]
        public async Task WhenTheInvalidAccountIdIsSubmitted()
        {
            var invalidAccountId = _fixture.Create<long>();
            _submitRequest.AccountId = invalidAccountId;
            var url = $"applications/{_submitRequest.IncentiveApplicationId}";
            _response =  await EmployerIncentiveApi.Patch(url, _submitRequest);
        }

    }
}
