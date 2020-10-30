using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CreateIncentive;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.CreateIncentive.Handlers
{
    public class WhenHandlingCreateApprenticeshipIncentiveCommand
    {
        private CreateApprenticeshipIncentiveCommandHandler _sut;
        private Mock<IApprenticeshipIncentiveDomainRepository> _mockIncentiveDomainRepository;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockIncentiveDomainRepository = new Mock<IApprenticeshipIncentiveDomainRepository>();

            _fixture.Customize(new ApprenticeshipIncentiveCustomization());
            _fixture.Customize(new IncentiveApplicationCustomization());
            _sut = new CreateApprenticeshipIncentiveCommandHandler(
                new ApprenticeshipIncentiveFactory(),
                _mockIncentiveDomainRepository.Object);
        }

        [Test]
        public async Task Then_an_apprenticeship_incentive_created_event_is_raised_for_each_apprenticeship_in_the_application()
        {
            // Arrange
            var command = _fixture.Create<CreateApprenticeshipIncentiveCommand>();

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
    }
}
