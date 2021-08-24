﻿using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CreateIncentive;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.CreateIncentive.Handlers
{
    public class WhenHandlingCreateApprenticeshipIncentiveCommand
    {
        private CreateIncentiveCommandHandler _sut;
        private Mock<IApprenticeshipIncentiveDomainRepository> _mockIncentiveDomainRepository;
        private Mock<ICommandPublisher> _mockCommandPublisher;
        private ApprenticeshipIncentiveFactory _factory;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockIncentiveDomainRepository = new Mock<IApprenticeshipIncentiveDomainRepository>();
            _mockCommandPublisher = new Mock<ICommandPublisher>();

            _fixture.Customize(new ApprenticeshipIncentiveCustomization());
            _fixture.Customize(new IncentiveApplicationCustomization());
            _factory = new ApprenticeshipIncentiveFactory();
            _sut = new CreateIncentiveCommandHandler(
                _factory,
                _mockIncentiveDomainRepository.Object,
                _mockCommandPublisher.Object);
        }

        [Test]
        public async Task Then_an_apprenticeship_incentive_created_event_is_raised_for_each_apprenticeship_in_the_application()
        {
            // Arrange
            var command = _fixture.Create<CreateIncentiveCommand>();
            Domain.ApprenticeshipIncentives.ApprenticeshipIncentive noExistingApprenticeshipIncentive = null;
            _mockIncentiveDomainRepository.Setup(x => x.FindByApprenticeshipId(command.IncentiveApplicationApprenticeshipId)).ReturnsAsync(noExistingApprenticeshipIncentive);

            // Act
            await _sut.Handle(command);

            // Assert
            _mockIncentiveDomainRepository.Verify(r =>
                r.Save(It.Is<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>(
                    i =>
                        i.Apprenticeship.Id == command.ApprenticeshipId &&
                        i.Apprenticeship.UniqueLearnerNumber == command.Uln &&
                        i.Apprenticeship.DateOfBirth == command.DateOfBirth &&
                        i.Apprenticeship.EmployerType == command.ApprenticeshipEmployerTypeOnApproval &&
                        i.Apprenticeship.FirstName == command.FirstName &&
                        i.Apprenticeship.LastName == command.LastName &&
                        i.Account.Id == command.AccountId &&
                        i.GetModel().SubmittedDate == command.SubmittedDate &&
                        i.GetModel().SubmittedByEmail == command.SubmittedByEmail &&
                        i.Apprenticeship.CourseName == command.CourseName &&
                        i.Apprenticeship.EmploymentStartDate == command.EmploymentStartDate &&
                        i.Phase.Identifier == command.Phase
                )), Times.Once());
        }

        [Test]
        public async Task Then_the_apprenticeship_incentive_is_not_created_if_one_exists_for_the_apprenticeship_id()
        {
            var command = _fixture.Create<CreateIncentiveCommand>();
            var existingAppenticeshipIncentive = (new ApprenticeshipIncentiveFactory()).CreateNew(Guid.NewGuid(), Guid.NewGuid(), _fixture.Create<Account>(), _fixture.Create<Apprenticeship>(), _fixture.Create<DateTime>(), _fixture.Create<DateTime>(), _fixture.Create<string>(), new AgreementVersion(_fixture.Create<int>()), new IncentivePhase(Phase.Phase1));
            _mockIncentiveDomainRepository.Setup(x => x.FindByApprenticeshipId(command.IncentiveApplicationApprenticeshipId)).ReturnsAsync(existingAppenticeshipIncentive);

            // Act
            await _sut.Handle(command);

            // Assert
            _mockIncentiveDomainRepository.Verify(r =>
                r.Save(It.IsAny<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>()), Times.Never());
        }

        [Test]
        public async Task Then_the_earnings_are_recorded_as_calculated_if_the_apprenticeship_incentive_exists_and_has_earnings()
        {
            var command = _fixture.Create<CreateIncentiveCommand>();
            
            var pendingPayments = new List<PendingPaymentModel>
            {
                _fixture.Create<PendingPaymentModel>(),
                _fixture.Create<PendingPaymentModel>()
            };

            var apprenticeshipIncentiveModel = _fixture.Create<ApprenticeshipIncentiveModel>();
            
            apprenticeshipIncentiveModel.PendingPaymentModels = pendingPayments;

            var existingAppenticeshipIncentive = (new ApprenticeshipIncentiveFactory()).GetExisting(Guid.NewGuid(), apprenticeshipIncentiveModel);

            _mockIncentiveDomainRepository.Setup(x => x.FindByApprenticeshipId(command.IncentiveApplicationApprenticeshipId)).ReturnsAsync(existingAppenticeshipIncentive);

            // Act
            await _sut.Handle(command);

            // Assert
            _mockCommandPublisher.Verify(r =>
                r.Publish(It.Is<CompleteEarningsCalculationCommand>(x => x.IncentiveApplicationApprenticeshipId == command.IncentiveApplicationApprenticeshipId), It.IsAny<CancellationToken>()), Times.Once());
        }

    }
}
