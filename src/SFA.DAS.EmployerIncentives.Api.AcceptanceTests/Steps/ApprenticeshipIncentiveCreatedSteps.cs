using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using NServiceBus.Transport;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Commands.Types.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "ApprenticeshipIncentiveCreated")]
    public class ApprenticeshipIncentiveCreatedSteps : StepsBase
    {
        private readonly TestContext _testContext;

        private readonly Account _accountModel;
        private readonly IncentiveApplication _applicationModel;
        private readonly Fixture _fixture;
        private readonly List<IncentiveApplicationApprenticeship> _apprenticeshipsModels;
        private readonly ApprenticeshipIncentive _apprenticeshipIncentive;
        private readonly PendingPayment _pendingPayment;
        private const int NumberOfApprenticeships = 3;

        public ApprenticeshipIncentiveCreatedSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();

            _accountModel = _fixture.Create<Account>();

            _applicationModel = _fixture.Build<IncentiveApplication>()
                .With(p => p.Status, IncentiveApplicationStatus.InProgress)
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
                .Create();

            _apprenticeshipsModels = _fixture.Build<IncentiveApplicationApprenticeship>()
                .With(p => p.IncentiveApplicationId, _applicationModel.Id)
                .With(p => p.PlannedStartDate, DateTime.Today.AddDays(1))
                .With(p => p.DateOfBirth, DateTime.Today.AddYears(-20))
                .With(p => p.EarningsCalculated, false)
                .CreateMany(NumberOfApprenticeships).ToList();

            _apprenticeshipIncentive = _fixture.Build<ApprenticeshipIncentive>()
                .With( p => p.IncentiveApplicationApprenticeshipId, _apprenticeshipsModels.First().Id)
                .With(p => p.AccountId, _applicationModel.AccountId)
                .With(p => p.ApprenticeshipId, _apprenticeshipsModels.First().ApprenticeshipId)
                .With(p => p.PlannedStartDate, DateTime.Today.AddDays(1))
                .With(p => p.DateOfBirth, DateTime.Today.AddYears(-20))
                .Create();

            _pendingPayment = _fixture.Build<PendingPayment>()
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.AccountId, _apprenticeshipIncentive.AccountId)
                .Create();
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

        [Given(@"an employer has submitted an application")]
        public async Task GivenAnEmployerHasSubmittedAnApplication()
        {
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.InsertAsync(_applicationModel);
                await dbConnection.InsertAsync(_apprenticeshipsModels);
            }
        }

        [Given(@"an apprenticeship incentive exists")]
        public async Task GivenAnApprenticeshipIncentiveExists()
        {
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.InsertAsync(_apprenticeshipIncentive);
            }
        }

        [Given(@"an apprenticeship incentive earnings have been calculated")]
        public async Task GivenAnApprenticeshipEarningsHaveBeenCalculated()
        {
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.InsertAsync(_accountModel);
                await dbConnection.InsertAsync(_applicationModel);
                await dbConnection.InsertAsync(_apprenticeshipsModels.First());

                await dbConnection.InsertAsync(_apprenticeshipIncentive);
                await dbConnection.InsertAsync(_pendingPayment);                
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

        [When(@"the apprenticeship incentive is created for each apprenticship in the application")]
        public async Task WhenTheApprenticeshipIncentiveIsCreatedForEachApprenticeshipInTheApplication()
        {
            var createCommand = new CreateIncentiveCommand(_applicationModel.AccountId, _applicationModel.Id);
       
            await EmployerIncentiveApi.PostCommand(
                    $"commands/ApprenticeshipIncentive.CreateIncentiveCommand",
                    createCommand);

            EmployerIncentiveApi.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
               
        [When(@"the apprenticeship incentive earnings are calculated")]
        public async Task WhenTheApprenticeshipIncentiveEarningsAreCalculated()
        {
            var calcEarningsCommand = new CalculateEarningsCommand(
                _apprenticeshipIncentive.Id,
                _apprenticeshipIncentive.AccountId,
                _apprenticeshipIncentive.ApprenticeshipId);

            await EmployerIncentiveApi.PostCommand(
                    $"commands/ApprenticeshipIncentive.CalculateEarningsCommand",
                    calcEarningsCommand);

            EmployerIncentiveApi.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [When(@"the earnings calculation against the apprenticeship incentive completes")]
        public async Task WhenTheEarningCalculationCompletes()
        {
            var completeEarningsCalcCommand = new CompleteEarningsCalculationCommand(
                _apprenticeshipIncentive.AccountId,
                _apprenticeshipIncentive.IncentiveApplicationApprenticeshipId,
                _apprenticeshipIncentive.ApprenticeshipId,
                _apprenticeshipIncentive.Id);

            //await EmployerIncentiveApi.PostCommand(
            //        $"commands/IncentiveApplications.CompleteEarningsCalculationCommand",
            //        completeEarningsCalcCommand);

            //EmployerIncentiveApi.Response.StatusCode.Should().Be(HttpStatusCode.OK);

            await _testContext.WaitFor<MessageContext>(async () =>
                await _testContext.MessageBus.Send(completeEarningsCalcCommand));
        }


        [Then(@"the apprenticeship incentive is created for the application")]
        public void ThenTheApprenticeshipIncentiveIsCreatedForTheApplication()
        {
            _testContext.CommandsPublished.Single(c => c.IsPublished).Command.Should().BeOfType<CreateIncentiveCommand>();
        }

        [Then(@"the earnings are calculated for each apprenticeship incentive")]
        public void ThenTheEarningsAreCalculatedForEachApprenticeshipIncentive()
        {
            var commandsPublished = _testContext.CommandsPublished.Where(c => c.IsPublished && c.Command.GetType() == typeof(CalculateEarningsCommand));

            commandsPublished.Count().Should().Be(NumberOfApprenticeships);

            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var createdIncentives = dbConnection.GetAll<ApprenticeshipIncentive>();

                createdIncentives.Count().Should().Be(NumberOfApprenticeships);
            }
        }

        [Then(@"the pending payments are stored against the apprenticeship incentive")]
        public void ThenThePendingPaymentsAreStoredAgainstTheApprenticeshipIncentive()
        {
            var completeCalculationCommandsPublished = _testContext.CommandsPublished.Where(c => c.IsPublished && c.Command.GetType() == typeof(CompleteEarningsCalculationCommand));

            completeCalculationCommandsPublished.Count().Should().Be(1);

            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var createdIncentives = dbConnection.GetAll<ApprenticeshipIncentive>();

                createdIncentives.Count().Should().Be(1);
                var pendingPayments = dbConnection.GetAll<PendingPayment>();

                pendingPayments.Where(p => p.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id).ToList().Count.Should().Be(2);
            }
        }

        [Then(@"the incentive application is updated to record that the earnings have been calculated")]
        public void ThenTheIncentiveApplicationIsUpdatedToRecordEarningsCalculated()
        {  
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var apprenticeshipApplications = dbConnection.GetAll<IncentiveApplicationApprenticeship>();

                apprenticeshipApplications.Count().Should().Be(1);
                var application = apprenticeshipApplications
                    .Single(a => a.IncentiveApplicationId == _applicationModel.Id &&
                    a.ApprenticeshipId == _apprenticeshipIncentive.ApprenticeshipId);

                application.EarningsCalculated.Should().BeTrue();
            }
        }
    }
}
