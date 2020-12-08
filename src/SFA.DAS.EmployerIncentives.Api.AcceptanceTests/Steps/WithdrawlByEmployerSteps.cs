using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "WithdrawlByEmployer")]
    public class WithdrawlByEmployerSteps : StepsBase
    {
        private readonly TestContext _testContext;
        private readonly Fixture _fixture;
        private readonly string _connectionString;
        private readonly IncentiveApplication _application;
        private readonly IncentiveApplicationApprenticeship _apprenticeship;
        private WithdrawApplicationRequest _withdrawApplicationRequest;

        public WithdrawlByEmployerSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();
            _connectionString = _testContext.SqlDatabase.DatabaseInfo.ConnectionString;

            _application = _fixture.Create<IncentiveApplication>();
            _apprenticeship = _fixture
                .Build<IncentiveApplicationApprenticeship>()
                .With(a => a.IncentiveApplicationId, _application.Id)
                .With(a => a.WithdrawnByEmployer, false)
                .Create();
        }

        [Given(@"an incentive application has been made without being submitted")]
        public async Task GivenAnIncentiveApplicationHasBeenMadeWithoutBeingSubmitted()
        {
            using var dbConnection = new SqlConnection(_connectionString);
            await dbConnection.InsertAsync(_application);
            await dbConnection.InsertAsync(_apprenticeship);
        }

        [When(@"the apprenticeship application is withdrawn from the scheme")]
        public async Task WhenTheApprenticeshipApplicationIsWithdrawnFromTheScheme()
        {
            _withdrawApplicationRequest = _fixture
                .Build<WithdrawApplicationRequest>()
                .With(r => r.WithdrawlType, WithdrawlType.Employer)
                .With(r => r.AccountLegalEntityId, _application.AccountLegalEntityId)
                .With(r => r.ULN, _apprenticeship.ULN)
                .Create();           
            
            var url = $"withdrawls";
            await EmployerIncentiveApi.Post(url, _withdrawApplicationRequest);
        }

        [Then(@"the incentive application status is updated to indicate the employer withdrawl")]
        public async Task ThenTheIncentiveApplicationStatusIsUpdatedToIndicateTheEmployerWithdrawl()
        {
            EmployerIncentiveApi.Response.StatusCode.Should().Be(HttpStatusCode.Accepted);

            await using var dbConnection = new SqlConnection(_connectionString);
            var apprenticeships = await dbConnection.GetAllAsync<IncentiveApplicationApprenticeship>();
            apprenticeships.Should().HaveCount(1);
            apprenticeships.Single(a => a.Id == _apprenticeship.Id).WithdrawnByEmployer.Should().BeTrue();
            
            var incentiveApplicationAudits = await dbConnection.GetAllAsync<IncentiveApplicationStatusAudit>();
            incentiveApplicationAudits.Should().HaveCount(1);
            var auditRecord = incentiveApplicationAudits.Single(a => a.IncentiveApplicationApprenticeshipId == _apprenticeship.Id);
            auditRecord.Process.Should().Be(IncentiveApplicationStatus.EmployerWithdrawn);
            auditRecord.ServiceRequestTaskId.Should().Be(_withdrawApplicationRequest.ServiceRequestTaskId);
            auditRecord.ServiceRequestDecisionReference.Should().Be(_withdrawApplicationRequest.ServiceRequestDecisionNumber);
            auditRecord.ServiceRequestCreatedDate.Should().Be(_withdrawApplicationRequest.ServiceRequestCreatedDate);

            var publishedCommand = _testContext
                .CommandsPublished
                .Single(c => c.IsPublished &&
                c.Command is WithdrawCommand).Command as WithdrawCommand;
                       
            publishedCommand.AccountId.Should().Be(_application.AccountId);
            publishedCommand.IncentiveApplicationApprenticeshipId.Should().Be(_apprenticeship.Id);
        }
    }
}
