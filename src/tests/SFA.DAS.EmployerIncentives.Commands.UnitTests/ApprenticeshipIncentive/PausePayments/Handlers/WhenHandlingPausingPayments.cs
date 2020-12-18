using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.PausePayments;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.PausePayments.Handlers
{
    public class WhenHandlingPausingPayments
    {
        private PausePaymentsCommandHandler _sut;
        private Mock<IApprenticeshipIncentiveDomainRepository> _mockDomainRepository;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize(new ApprenticeshipIncentiveCustomization());

            _mockDomainRepository = new Mock<IApprenticeshipIncentiveDomainRepository>();

            _sut = new PausePaymentsCommandHandler(_mockDomainRepository.Object);
        }

        [Test]
        public async Task Then_the_KeyNotFoundException_is_thrown_when_no_incentive_is_found_for_this_uln_and_accountlegalentityid()
        {
            var command = _fixture.Create<PausePaymentsCommand>();

            Func<Task> act = async () => await _sut.Handle(command); 

            act.Should().Throw<KeyNotFoundException>();
        }

        [Test]
        public async Task Then_the_PausePayments_flag_for_this_incentive_is_set_on()
        {
            // Arrange
            var command = CreatePausedPaymentsCommandWithActionPause();
            var apprenticeshipIncentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();

            _mockDomainRepository
                .Setup(x => x.FindByUlnWithinAccountLegalEntity(command.ULN, command.AccountLegalEntityId))
                .ReturnsAsync(apprenticeshipIncentive);

            // Act
            await _sut.Handle(command);

            // Assert
            _mockDomainRepository.Verify(x => x.Save(It.Is<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>(x => x.PausePayments == true)), Times.Once);
        }

        [Test]
        public async Task Then_an_PaymentPaused_event_is_raised_when_the_incentive_is_paused()
        {
            // Arrange
            var command = CreatePausedPaymentsCommandWithActionPause();
            var apprenticeshipIncentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();

            _mockDomainRepository
                .Setup(x => x.FindByUlnWithinAccountLegalEntity(command.ULN, command.AccountLegalEntityId))
                .ReturnsAsync(apprenticeshipIncentive);

            // Act
            await _sut.Handle(command);

            // Assert
            var raisedEvent = apprenticeshipIncentive.FlushEvents().OfType<PaymentsPaused>().Single();
            raisedEvent.AccountId.Should().Be(apprenticeshipIncentive.Account.Id);
            raisedEvent.AccountLegalEntityId.Should().Be(apprenticeshipIncentive.Account.AccountLegalEntityId);
            raisedEvent.ServiceRequest.Created.Should().Be(command.DateServiceRequestTaskCreated.Value);
            raisedEvent.ServiceRequest.DecisionReference.Should().Be(command.DecisionReferenceNumber);
            raisedEvent.ServiceRequest.TaskId.Should().Be(command.ServiceRequestId);
        }

        [Test]
        public async Task Then_the_PausePaymentsException_is_thrown_when_the_incentive_is_already_paused()
        {
            // Arrange
            var command = CreatePausedPaymentsCommandWithActionPause();
            var apprenticeshipIncentive = Domain.ApprenticeshipIncentives.ApprenticeshipIncentive.New(
                _fixture.Create<Guid>(), _fixture.Create<Guid>(), _fixture.Create<Account>(),
                _fixture.Create<Apprenticeship>(), _fixture.Create<DateTime>(), true);
            _mockDomainRepository
                .Setup(x => x.FindByUlnWithinAccountLegalEntity(command.ULN, command.AccountLegalEntityId))
                .ReturnsAsync(apprenticeshipIncentive);

            // Act
            Func<Task> act = async () => await _sut.Handle(command);

            // Assert
            act.Should().Throw<PausePaymentsException>().WithMessage("Payments already paused");
        }

        private PausePaymentsCommand CreatePausedPaymentsCommandWithActionPause()
        {
            return new PausePaymentsCommand(_fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<string>(),
                _fixture.Create<string>(), _fixture.Create<DateTime>(), PausePaymentsAction.Pause);
        }
    }
}
