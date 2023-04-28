using System;
using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "ReinstateWithdrawal")]
    public class ReinstateWithdrawalSteps : StepsBase
    {
        private readonly TestContext _testContext;
        private readonly Fixture _fixture;
        private readonly string _connectionString;
        private readonly IncentiveApplication _application;
        private readonly IncentiveApplicationApprenticeship _apprenticeship;
        private ReinstateApplicationRequest _reinstateApplicationRequest;

        private readonly ApprenticeshipIncentive _apprenticeshipIncentive;
        private HttpResponseMessage _response;

        public ReinstateWithdrawalSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();
            _connectionString = _testContext.SqlDatabase.DatabaseInfo.ConnectionString;

            _application = _fixture.Build<IncentiveApplication>()
                .With(x => x.Status, IncentiveApplicationStatus.Submitted)
                .Create();

            _apprenticeship = _fixture
                .Build<IncentiveApplicationApprenticeship>()
                .With(a => a.IncentiveApplicationId, _application.Id)
                .With(a => a.WithdrawnByCompliance, true)
                .With(a => a.Phase, Phase.Phase2)
                .With(a => a.PlannedStartDate, new DateTime(2021, 6, 1))
                .Create();

            _apprenticeshipIncentive = _fixture
                .Build<ApprenticeshipIncentive>()
                .With(i => i.IncentiveApplicationApprenticeshipId, _apprenticeship.Id)
                .With(i => i.AccountLegalEntityId, _application.AccountLegalEntityId)
                .With(i => i.AccountId, _application.AccountId)
                .With(i => i.ULN, _apprenticeship.ULN)
                .With(i => i.Status, IncentiveStatus.Withdrawn)
                .With(i => i.Phase, Phase.Phase2)
                .With(i => i.StartDate, new DateTime(2021, 6, 1))
                .Without(i => i.PendingPayments)
                .Without(i => i.Payments)
                .Create();
        }

        [Given(@"an incentive application has been withdrawn by compliance")]
        public async Task GivenAnIncentiveApplicationHasBeenWithdrawnByCompliance()
        {
            using var dbConnection = new SqlConnection(_connectionString);
            await dbConnection.InsertAsync(_application);
            await dbConnection.InsertAsync(_apprenticeship);
            await dbConnection.InsertAsync(_apprenticeshipIncentive);
        }

        [When(@"a request is made to reinstate the application")]
        public async Task WhenARequestIsMadeToReinstateTheApplication()
        {
            var reinstateApplication = _fixture
                .Build<Application>()
                .With(r => r.AccountLegalEntityId, _application.AccountLegalEntityId)
                .With(r => r.ULN, _apprenticeship.ULN)
                .Create();

            _reinstateApplicationRequest = new ReinstateApplicationRequest { Applications = new[] { reinstateApplication } };

            var url = $"withdrawal-reinstatements";

            await _testContext.WaitFor(
                async (cancellationToken) =>
                {
                    _response = await EmployerIncentiveApi.Post(url, _reinstateApplicationRequest, cancellationToken);
                },
                (context) => HasExpectedEvents(context)
                );
        }

        private bool HasExpectedEvents(TestContext testContext)
        {
            var processedEvents = testContext.CommandsPublished.Count(c => c.IsProcessed && c.Command is ReinstateApprenticeshipIncentiveCommand);
            return processedEvents == 1;
        }

        [Then(@"the incentive application status is reset")]
        public async Task ThenTheIncentiveApplicationStatusIsReset()
        {
            _response.StatusCode.Should().Be(HttpStatusCode.Accepted);

            await using var dbConnection = new SqlConnection(_connectionString);
            var apprenticeships = await dbConnection.GetAllAsync<IncentiveApplicationApprenticeship>();
            apprenticeships.Should().HaveCount(1);
            apprenticeships.Single(a => a.Id == _apprenticeship.Id).WithdrawnByCompliance.Should().BeFalse();

            var incentiveApplicationAudits = await dbConnection.GetAllAsync<IncentiveApplicationStatusAudit>();
            incentiveApplicationAudits.Should().HaveCount(1);
            var auditRecord = incentiveApplicationAudits.Single(a => a.IncentiveApplicationApprenticeshipId == _apprenticeship.Id);
            auditRecord.Process.Should().Be(IncentiveApplicationStatus.Reinstated);

            var publishedCommand = _testContext
                .CommandsPublished
                .Single(c => c.IsPublished &&
                c.Command is ReinstateApprenticeshipIncentiveCommand).Command as ReinstateApprenticeshipIncentiveCommand;

            publishedCommand.AccountId.Should().Be(_application.AccountId);
            publishedCommand.IncentiveApplicationApprenticeshipId.Should().Be(_apprenticeship.Id);
        }

        [Then(@"the apprenticeship incentive status is reset to Paused")]
        public async Task ThenTheApprenticeshipIncentiveIsStatusIsResetToPaused()
        {
            await using var dbConnection = new SqlConnection(_connectionString);
            var incentives = await dbConnection.GetAllAsync<ApprenticeshipIncentive>();

            incentives.Should().HaveCount(1);
            var incentive = incentives.FirstOrDefault();
            incentive.Status.Should().Be(IncentiveStatus.Paused);
            incentive.WithdrawnBy.Should().BeNull();
            incentive.PausePayments.Should().BeTrue();
        }

        [Then(@"the earnings are recalculated")]
        public async Task ThenTheEarningsAreRecalculated()
        {
            await using var dbConnection = new SqlConnection(_connectionString);
            var pendingPayments = await dbConnection.GetAllAsync<PendingPayment>();

            pendingPayments.Should().HaveCount(2);
        }
    }
}
