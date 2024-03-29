﻿using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Commands.Types.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
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
        private DateTime _today;
        private const int NumberOfApprenticeships = 3;

        public ApprenticeshipIncentiveCreatedSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();
            _today = new DateTime(2021, 6, 30);

            _accountModel = _fixture.Create<Account>();

            _applicationModel = _fixture.Build<IncentiveApplication>()
                .With(p => p.Status, IncentiveApplicationStatus.InProgress)
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
                .Create();

            _apprenticeshipsModels = _fixture.Build<IncentiveApplicationApprenticeship>()
                .With(p => p.IncentiveApplicationId, _applicationModel.Id)
                .With(p => p.PlannedStartDate, _today.AddMonths(-2))
                .With(p => p.DateOfBirth, _today.AddYears(-20))
                .With(p => p.EarningsCalculated, false)
                .With(p => p.WithdrawnByCompliance, false)
                .With(p => p.WithdrawnByEmployer, false)
                .With(p => p.Phase, Phase.Phase1)
                .CreateMany(NumberOfApprenticeships).ToList();

            _apprenticeshipIncentive = _fixture.Build<ApprenticeshipIncentive>()
                .With(p => p.IncentiveApplicationApprenticeshipId, _apprenticeshipsModels.First().Id)
                .With(p => p.AccountId, _applicationModel.AccountId)
                .With(p => p.AccountLegalEntityId, _applicationModel.AccountLegalEntityId)
                .With(p => p.ApprenticeshipId, _apprenticeshipsModels.First().ApprenticeshipId)
                .With(p => p.StartDate, _today.AddDays(1))
                .With(p => p.SubmittedDate, _today)
                .With(p => p.DateOfBirth, _today.AddYears(-20))
                .With(p => p.Phase, Phase.Phase2)
                .Create();

            _pendingPayment = _fixture.Build<PendingPayment>()
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.AccountId, _apprenticeshipIncentive.AccountId)
                .With(p => p.AccountLegalEntityId, _apprenticeshipIncentive.AccountLegalEntityId)
                .Create();
        }

        [Given(@"an employer is applying for the New Apprenticeship Incentive")]
        public async Task GivenAnEmployerIsApplyingForTheNewApprenticeshipIncentive()
        {
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.InsertAsync(_accountModel);
                await dbConnection.InsertAsync(_applicationModel);
                await dbConnection.InsertAsync(_apprenticeshipsModels);
            }
        }

        [Given(@"an existing withdrawn incentive")]
        public async Task GivenAnExistingWithdrawnIncentive()
        {
            _apprenticeshipIncentive.Status = IncentiveStatus.Withdrawn;
            _apprenticeshipIncentive.IncentiveApplicationApprenticeshipId = Guid.NewGuid();
            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(_apprenticeshipIncentive, true);
        }

        [Given(@"an employer is re-applying for apprenticeship incentive")]
        public async Task GivenAnEmployerIsRe_ApplyingForApprenticeshipIncentive()
        {
            _apprenticeshipsModels.RemoveRange(1, _apprenticeshipsModels.Count - 1);
            await GivenAnEmployerIsApplyingForTheNewApprenticeshipIncentive();
        }

        [Given(@"an employer has submitted an application")]
        public async Task GivenAnEmployerHasSubmittedAnApplication()
        {
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.InsertAsync(_accountModel);
                await dbConnection.InsertAsync(_applicationModel);
                await dbConnection.InsertAsync(_apprenticeshipsModels);
            }
        }

        [Given(@"an apprenticeship incentive exists")]
        public async Task GivenAnApprenticeshipIncentiveExists()
        {
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.InsertAsync(_accountModel);
                await dbConnection.InsertAsync(_applicationModel);
                await dbConnection.InsertAsync(_apprenticeshipsModels);
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

        [When(@"the apprenticeship incentive is created for each apprenticeship in the application")]
        [Then(@"the apprenticeship incentive is created for each apprenticeship in the application")]
        public async Task WhenTheApprenticeshipIncentiveIsCreatedForEachApprenticeshipInTheApplication()
        {
            foreach (var apprenticeship in _apprenticeshipsModels)
            {
                var createCommand = new CreateIncentiveCommand(_applicationModel.AccountId, _applicationModel.AccountLegalEntityId,
                    apprenticeship.Id,
                    apprenticeship.ApprenticeshipId,
                    apprenticeship.FirstName,
                    apprenticeship.LastName,
                    apprenticeship.DateOfBirth,
                    apprenticeship.ULN,
                    apprenticeship.PlannedStartDate,
                    apprenticeship.ApprenticeshipEmployerTypeOnApproval,
                    apprenticeship.UKPRN,
                    _applicationModel.DateSubmitted.Value,
                    _applicationModel.SubmittedByEmail,
                    apprenticeship.CourseName,
                    apprenticeship.EmploymentStartDate.Value,
                    apprenticeship.Phase);

                await _testContext.WaitFor<ICommand>(async (cancellationToken) =>
                   await _testContext.MessageBus.Send(createCommand), numberOfOnProcessedEventsExpected: _apprenticeshipsModels.Count());
            }
        }

        [When(@"the apprenticeship incentive earnings are calculated")]
        public async Task WhenTheApprenticeshipIncentiveEarningsAreCalculated()
        {
            var calcEarningsCommand = new CalculateEarningsCommand(_apprenticeshipIncentive.Id);

            await _testContext.WaitFor(
                async (cancellationToken) =>
                {
                    await _testContext.MessageBus.Send(calcEarningsCommand);
                },
                (context) => HasExpectedCompleteEarningsCalculationEvents(context)
                );
        }

        private bool HasExpectedCompleteEarningsCalculationEvents(TestContext testContext)
        {
            var processedEvents = testContext.CommandsPublished.Count(c =>
            c.IsProcessed &&
            c.IsDomainCommand &&
            c.Command is CompleteEarningsCalculationCommand);

            return processedEvents == 1;
        }

        [When(@"the earnings calculation against the apprenticeship incentive completes")]
        public async Task WhenTheEarningCalculationCompletes()
        {
            var completeEarningsCalcCommand = new CompleteEarningsCalculationCommand(
                _apprenticeshipIncentive.AccountId,
                _apprenticeshipIncentive.IncentiveApplicationApprenticeshipId,
                _apprenticeshipIncentive.ApprenticeshipId,
                _apprenticeshipIncentive.Id);

            await _testContext.WaitFor<ICommand>(async (cancellationToken) =>
                await _testContext.MessageBus.Send(completeEarningsCalcCommand));
        }


        [Then(@"the apprenticeship incentive is created for the application")]
        public void ThenTheApprenticeshipIncentiveIsCreatedForTheApplication()
        {
            var publishedCommands = _testContext.CommandsPublished
                .Where(c => c.IsPublished && 
                c.IsDomainCommand &&
                c.Command is CreateIncentiveCommand)
                .Select(c => c.Command)
                .ToArray();

            foreach (var publishedCommand in publishedCommands)
            {
                publishedCommand.Should().BeOfType<CreateIncentiveCommand>();
                var command = publishedCommand as CreateIncentiveCommand;
                Debug.Assert(command != null, nameof(command) + " != null");
                command.AccountId.Should().Be(_applicationModel.AccountId);
                command.AccountLegalEntityId.Should().Be(_applicationModel.AccountLegalEntityId);
                command.LockId.Should().Be($"{nameof(Account)}_{command.AccountId}");
            }
        }

        [Then(@"the earnings are calculated for each apprenticeship incentive")]
        public void ThenTheEarningsAreCalculatedForEachApprenticeshipIncentive()
        {
            var commandsPublished = _testContext.CommandsPublished
                .Where(c => 
                c.IsPublished &&
                c.IsDomainCommand &&
                c.Command.GetType() == typeof(CalculateEarningsCommand));

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
            var completeCalculationCommandsPublished = _testContext.CommandsPublished
                .Where(c => c.IsProcessed && 
                c.IsDomainCommand &&
                c.Command.GetType() == typeof(CompleteEarningsCalculationCommand));

            completeCalculationCommandsPublished.Count().Should().Be(1);

            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var createdIncentives = dbConnection.GetAll<ApprenticeshipIncentive>();

                createdIncentives.Count().Should().Be(1);
                var pendingPayments = dbConnection.GetAll<PendingPayment>();

                pendingPayments.Where(p => p.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id).ToList().Count.Should().Be(2);
                pendingPayments.ToList().ForEach(p => p.AccountLegalEntityId.Should().Be(_accountModel.AccountLegalEntityId));
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

        [Given(@"an apprenticeship incentive exists with a payment due within the delay period")]
        public async Task GivenAnApprenticeshipIncentiveExistsWithAPaymentDueWithinTheDelayPeriod()
        {
            _apprenticeshipIncentive.SubmittedDate = _today.AddDays(90);
            await GivenAnApprenticeshipIncentiveExists();
        }

        [Then(@"the first pending payment is due at the end of the delay period")]
        public void ThenTheFirstPendingPaymentIsDueAtTheEndOfTheDelayPeriod()
        {
            var completeCalculationCommandsPublished = _testContext.CommandsPublished
                .Where(c => c.IsProcessed &&
                            c.IsDomainCommand &&
                            c.Command.GetType() == typeof(CompleteEarningsCalculationCommand));

            completeCalculationCommandsPublished.Count().Should().Be(1);

            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var createdIncentives = dbConnection.GetAll<ApprenticeshipIncentive>();

                createdIncentives.Count().Should().Be(1);
                var pendingPayments = dbConnection.GetAll<PendingPayment>();

                var expectedDueDate = _apprenticeshipIncentive.SubmittedDate.Value.AddDays(21);

                var firstPendingPayment = pendingPayments.Single(p => p.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id && p.EarningType == EarningType.FirstPayment);
                firstPendingPayment.DueDate.Date.Should().Be(expectedDueDate.Date);
            }
        }

        [Then(@"original withdrawn incentive is retained")]
        public void ThenOriginalWithdrawnIncentiveIsRetained()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var incentives = dbConnection.GetAll<ApprenticeshipIncentive>().ToList();
            incentives.Count().Should().Be(2);
            incentives.Count(a => a.Status == IncentiveStatus.Withdrawn).Should().Be(1);
            incentives.Count(a => a.Status == IncentiveStatus.Active).Should().Be(1);
        }
    }
}
