using System;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "MonthEndInProgress")]
    public class MonthEndInProgressSteps : StepsBase
    {
        private readonly TestContext _testContext;
        private readonly Fixture _fixture;
        private readonly ApprenticeshipIncentive _apprenticeshipIncentive;
        private readonly IncentiveApplication _application;
        private readonly IncentiveApplicationApprenticeship _applicationApprenticeship;
        private readonly Account _accountModel;
        private HttpResponseMessage _response;

        public MonthEndInProgressSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();
            var today = new DateTime(2021, 1, 30);

            _accountModel = _fixture.Create<Account>();

            _application = _fixture.Build<IncentiveApplication>()
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
                .Create();

            _applicationApprenticeship = _fixture
                .Build<IncentiveApplicationApprenticeship>()
                .With(a => a.IncentiveApplicationId, _application.Id)
                .With(a => a.WithdrawnByEmployer, false)
                .With(a => a.WithdrawnByCompliance, false)
                .Create();

            _apprenticeshipIncentive = _fixture.Build<ApprenticeshipIncentive>()
                .With(p => p.IncentiveApplicationApprenticeshipId, _applicationApprenticeship.Id)
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
                .With(p => p.StartDate, today.AddDays(1))
                .With(p => p.DateOfBirth, today.AddYears(-20))
                .With(p => p.Status, IncentiveStatus.Active)
                .Create();            
        }

        [Given(@"an apprenticeship incentive exists")]
        public async Task GivenAnApprenticeshipIncentiveExists()
        {
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.InsertAsync(_application);
                await dbConnection.InsertAsync(_applicationApprenticeship);
                await dbConnection.InsertAsync(_accountModel);
                await dbConnection.InsertAsync(_apprenticeshipIncentive);
            }
        }

        [Given(@"the active collection period is currently in progress")]
        public async Task GivenTheActiveCollectionPeriodIsCurrentlyInProgress()
        {
            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            _testContext.ActivePeriod.PeriodEndInProgress = true;
            await dbConnection.UpdateAsync(_testContext.ActivePeriod);
        }

        [When(@"an earnings calculation is requested")]
        public async Task WhenAnEarningsCalculationIsRequested()
        {
            var calcEarningsCommand = new CalculateEarningsCommand(_apprenticeshipIncentive.Id);

            await _testContext.WaitFor<ICommand>(async (cancellationToken) =>
                await _testContext.MessageBus.Send(calcEarningsCommand), numberOfOnProcessedEventsExpected: 1);
        }

        [When(@"an employer withdrawal is requested")]
        public async Task WhenAnEmployerWithdrawalIsRequested()
        {
            var withdrawApplicationRequest = _fixture
                .Build<WithdrawApplicationRequest>()
                .With(r => r.WithdrawalType, WithdrawalType.Employer)
                .With(r => r.AccountLegalEntityId, _application.AccountLegalEntityId)
                .With(r => r.ULN, _apprenticeshipIncentive.ULN)
                .Create();

            var url = $"withdrawals";

            await _testContext.WaitFor(
                async (cancellationToken) =>
                {
                    _response = await EmployerIncentiveApi.Post(url,withdrawApplicationRequest, cancellationToken);
                },
                (context) => HasExpectedEmployerWithdrawEvents(context)
                );
        }

        [When(@"a compliance withdrawal is requested")]
        public async Task WhenAComplianceWithdrawalIsRequested()
        {
            var withdrawApplicationRequest = _fixture
                .Build<WithdrawApplicationRequest>()
                .With(r => r.WithdrawalType, WithdrawalType.Compliance)
                .With(r => r.AccountLegalEntityId, _application.AccountLegalEntityId)
                .With(r => r.ULN, _apprenticeshipIncentive.ULN)
                .Create();

            var url = $"withdrawals";

            await _testContext.WaitFor(
                async (cancellationToken) =>
                {
                    _response = await EmployerIncentiveApi.Post(url, withdrawApplicationRequest, cancellationToken);
                },
                (context) => HasExpectedComplianceWithdrawEvents(context)
            );
        }

        private bool HasExpectedEmployerWithdrawEvents(TestContext testContext)
        {   
            var processedCommands = testContext.CommandsPublished.Count(c => c.IsProcessed && c.Command is Commands.Types.Withdrawals.EmployerWithdrawalCommand);
            return processedCommands == 1;
        }

        private bool HasExpectedComplianceWithdrawEvents(TestContext testContext)
        {
            var processedCommands = testContext.CommandsPublished.Count(c => c.IsProcessed && c.Command is Commands.Types.Withdrawals.ComplianceWithdrawalCommand);
            return processedCommands == 1;
        }


        [Then(@"the earnings calculation is deferred")]
        public void ThenTheEarningsCalculationIsDeferred()
        {
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var pendingPayments = dbConnection.GetAll<PendingPayment>().Where(x => x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id);

                pendingPayments.Count().Should().Be(0);
            }

            var delayedCalculateEarningsCommands = _testContext.CommandsPublished
                .Where(c => c.IsDelayed && c.Command is CalculateEarningsCommand);

            delayedCalculateEarningsCommands.Count().Should().Be(1);
            ((CalculateEarningsCommand)delayedCalculateEarningsCommands.Single().Command).CommandDelay.Should().BeGreaterThan(TimeSpan.FromMinutes(55));
        }

        [Then(@"the employer withdrawal is deferred")]
        public void ThenTheEmployerWithdrawalIsDeferred()
        {
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var apprenticeApplications = dbConnection.GetAll<IncentiveApplicationApprenticeship>().Where(x => x.Id == _applicationApprenticeship.Id);

                apprenticeApplications.Count().Should().Be(1);

                apprenticeApplications.Single().WithdrawnByEmployer.Should().BeFalse();
            }

            var delayedWithdrawCommands = _testContext.CommandsPublished
                .Where(c => c.IsDelayed && c.Command is Commands.Types.Withdrawals.EmployerWithdrawalCommand);

            delayedWithdrawCommands.Count().Should().Be(1);            
            ((Commands.Types.Withdrawals.EmployerWithdrawalCommand)delayedWithdrawCommands.Single().Command).CommandDelay.Should().BeGreaterThan(TimeSpan.FromMinutes(12));
        }

        [Then(@"the compliance withdrawal is deferred")]
        public void ThenTheComplianceWithdrawalIsDeferred()
        {
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var apprenticeApplications = dbConnection.GetAll<IncentiveApplicationApprenticeship>().Where(x => x.Id == _applicationApprenticeship.Id);

                apprenticeApplications.Count().Should().Be(1);

                apprenticeApplications.Single().WithdrawnByCompliance.Should().BeFalse();
            }

            var delayedWithdrawCommands = _testContext.CommandsPublished
                .Where(c => c.IsDelayed && c.Command is Commands.Types.Withdrawals.ComplianceWithdrawalCommand);

            delayedWithdrawCommands.Count().Should().Be(1);
            ((Commands.Types.Withdrawals.ComplianceWithdrawalCommand)delayedWithdrawCommands.Single().Command).CommandDelay.Should().BeGreaterThan(TimeSpan.FromMinutes(12));
        }
    }
}
