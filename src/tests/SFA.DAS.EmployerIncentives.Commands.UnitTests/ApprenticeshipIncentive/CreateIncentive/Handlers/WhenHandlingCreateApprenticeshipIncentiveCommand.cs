using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CreateIncentive;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.CreateIncentive.Handlers
{
    public class WhenHandlingCreateApprenticeshipIncentiveCommand
    {
        private CreateIncentiveCommandHandler _sut;
        private Mock<IApprenticeshipIncentiveDomainRepository> _mockIncentiveDomainRepository;
        private ApprenticeshipIncentiveFactory _factory;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockIncentiveDomainRepository = new Mock<IApprenticeshipIncentiveDomainRepository>();

            _fixture.Customize(new ApprenticeshipIncentiveCustomization());
            _fixture.Customize(new IncentiveApplicationCustomization());
            _factory = new ApprenticeshipIncentiveFactory();
            _sut = new CreateIncentiveCommandHandler(
                _factory,
                _mockIncentiveDomainRepository.Object);
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
                        i.GetModel().SubmittedByEmail == command.SubmittedByEmail
                )), Times.Once());
        }

        [Test]
        public async Task Then_the_apprenticeship_incentive_is_not_created_if_one_exists_for_the_apprenticeship_id()
        {
            var command = _fixture.Create<CreateIncentiveCommand>();
            var existingAppenticeshipIncentive = (new ApprenticeshipIncentiveFactory()).CreateNew(Guid.NewGuid(), Guid.NewGuid(), _fixture.Create<Account>(), _fixture.Create<Apprenticeship>(), _fixture.Create<DateTime>(), _fixture.Create<DateTime>(), _fixture.Create<string>());
            _mockIncentiveDomainRepository.Setup(x => x.FindByApprenticeshipId(command.IncentiveApplicationApprenticeshipId)).ReturnsAsync(existingAppenticeshipIncentive);

            // Act
            await _sut.Handle(command);

            // Assert
            _mockIncentiveDomainRepository.Verify(r =>
                r.Save(It.IsAny<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>()), Times.Never());
        }

    }
}
