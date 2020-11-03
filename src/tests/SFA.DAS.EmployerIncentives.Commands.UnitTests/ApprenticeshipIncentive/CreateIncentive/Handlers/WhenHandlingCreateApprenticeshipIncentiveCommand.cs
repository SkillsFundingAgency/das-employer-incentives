using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CreateIncentive;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.CreateIncentive.Handlers
{
    public class WhenHandlingCreateApprenticeshipIncentiveCommand
    {
        private CreateApprenticeshipIncentiveCommandHandler _sut;
        private Mock<IApprenticeshipIncentiveDomainRepository> _mockIncentiveDomainRepository;
        private Mock<IDomainEventDispatcher> _mockDomainEventDispatcher;
        private ApprenticeshipIncentiveFactory _factory;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockIncentiveDomainRepository = new Mock<IApprenticeshipIncentiveDomainRepository>();
            _mockDomainEventDispatcher = new Mock<IDomainEventDispatcher>();

            _fixture.Customize(new ApprenticeshipIncentiveCustomization());
            _fixture.Customize(new IncentiveApplicationCustomization());
            _factory = new ApprenticeshipIncentiveFactory();
            _sut = new CreateApprenticeshipIncentiveCommandHandler(
                _factory,
                _mockIncentiveDomainRepository.Object,
                _mockDomainEventDispatcher.Object);
        }

        [Test]
        public async Task Then_an_apprenticeship_incentive_created_event_is_raised_for_each_apprenticeship_in_the_application()
        {
            // Arrange
            var command = _fixture.Create<CreateApprenticeshipIncentiveCommand>();
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
                        i.Account.Id == command.AccountId
                )), Times.Once());
        }

        [Test]
        public async Task Then_a_calculate_payments_event_is_triggered_if_the_apprenticeship_incentive_has_none_setup()
        {
            // Arrange
            var command = _fixture.Create<CreateApprenticeshipIncentiveCommand>();
            var existingApprenticeshipIncentive = _factory.CreateNew(Guid.NewGuid(), Guid.NewGuid(), _fixture.Create<Account>(), _fixture.Create<Apprenticeship>(), _fixture.Create<DateTime>());
            _mockIncentiveDomainRepository.Setup(x => x.FindByApprenticeshipId(command.IncentiveApplicationApprenticeshipId)).ReturnsAsync(existingApprenticeshipIncentive);

            // Act
            await _sut.Handle(command);

            // Assert
            _mockIncentiveDomainRepository.Verify(r =>
                r.Save(It.IsAny<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>()), Times.Never());
            _mockDomainEventDispatcher.Verify(x => x.Send(It.IsAny<Created>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Then_the_repository_is_not_updated_if_the_apprenticeship_incentive_and_payments_already_exist()
        {
            // Arrange
            var command = _fixture.Create<CreateApprenticeshipIncentiveCommand>();
            var existingApprenticeshipIncentive = _factory.CreateNew(Guid.NewGuid(), Guid.NewGuid(), _fixture.Create<Account>(), _fixture.Create<Apprenticeship>(), new DateTime(2020, 09, 01));
            
            var paymentProfiles = _fixture.CreateMany<IncentivePaymentProfile>(2);
            existingApprenticeshipIncentive.CalculateEarnings(paymentProfiles);
            _mockIncentiveDomainRepository.Setup(x => x.FindByApprenticeshipId(command.IncentiveApplicationApprenticeshipId)).ReturnsAsync(existingApprenticeshipIncentive);

            // Act
            await _sut.Handle(command);

            // Assert
            _mockIncentiveDomainRepository.Verify(r =>
                r.Save(It.IsAny<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>()), Times.Never());
            _mockDomainEventDispatcher.Verify(x => x.Send(It.IsAny<Created>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
