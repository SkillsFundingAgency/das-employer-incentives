using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
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
        private Account _accountModel;

        public MonthEndInProgressSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();
            var today = new DateTime(2021, 1, 30);

            _accountModel = _fixture.Create<Account>();

            _apprenticeshipIncentive = _fixture.Build<ApprenticeshipIncentive>()
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
                .With(p => p.StartDate, today.AddDays(1))
                .With(p => p.DateOfBirth, today.AddYears(-20))
                .Create();
        }

        [Given(@"an apprenticeship incentive exists")]
        public async Task GivenAnApprenticeshipIncentiveExists()
        {
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
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

        [Then(@"the earnings calculation is deferred")]
        public async Task ThenTheEarningsCalculationIsDeferred()
        {
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var pendingPayments = dbConnection.GetAll<PendingPayment>().Where(x => x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id);

                pendingPayments.Count().Should().Be(0);
            }

            var calculateEarningsCommands = _testContext.EventsPublished
                .Where(c => c.GetType() == typeof(CalculateEarningsCommand));

            calculateEarningsCommands.Count().Should().Be(1);
        }
    }
}
