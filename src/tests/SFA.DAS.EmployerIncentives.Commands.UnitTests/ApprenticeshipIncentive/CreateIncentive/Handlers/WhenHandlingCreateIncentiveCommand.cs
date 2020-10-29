using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CreateIncentive;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.CreateIncentive.Handlers
{
    public class WhenHandlingCreateIncentiveCommand
    {
        private CreateIncentiveCommandHandler _sut;
        private Mock<IApprenticeshipIncentiveDomainRepository> _mockIncentiveDomainRepository;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockIncentiveDomainRepository = new Mock<IApprenticeshipIncentiveDomainRepository>();

            _fixture.Customize(new ApprenticeshipIncentiveCustomization());
            _fixture.Customize(new IncentiveApplicationCustomization());
            _sut = new CreateIncentiveCommandHandler(
                new ApprenticeshipIncentiveFactory(),
                _mockIncentiveDomainRepository.Object);
        }

        [Test]
        public async Task Then_an_apprenticeship_incentive_created_event_is_raised_for_each_apprenticeship_in_the_application()
        {
            // Arrange
            const int numberOfApprenticeships = 5;
            var apprenticeships = _fixture.CreateMany<CreateIncentiveCommand.IncentiveApprenticeship>(numberOfApprenticeships).ToList();
            var incentiveApplicationAccountId = _fixture.Create<long>();
            var incentiveApplicationAccountLegalEntityId = _fixture.Create<long>();
            var command = new CreateIncentiveCommand(incentiveApplicationAccountId, incentiveApplicationAccountLegalEntityId, apprenticeships);

            // Act
            await _sut.Handle(command);

            // Assert
            _mockIncentiveDomainRepository.Verify(r => r.Save(It.IsAny<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>())
            , Times.Exactly(numberOfApprenticeships));

            foreach (var apprenticeship in apprenticeships)
            {
                _mockIncentiveDomainRepository.Verify(r =>
                    r.Save(It.Is<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>(
                        i =>
                            i.Apprenticeship.Id == apprenticeship.ApprenticeshipId &&
                            i.Apprenticeship.UniqueLearnerNumber == apprenticeship.Uln &&
                            i.Apprenticeship.DateOfBirth == apprenticeship.DateOfBirth &&
                            i.Apprenticeship.EmployerType == apprenticeship.ApprenticeshipEmployerTypeOnApproval &&
                            i.Apprenticeship.FirstName == apprenticeship.FirstName &&
                            i.Apprenticeship.LastName == apprenticeship.LastName &&
                            i.Account.Id == incentiveApplicationAccountId
                    )), Times.Once());
            }
        }

        [Test]
        public async Task Then_an_apprenticeship_incentive_is_persisted_for_each_apprenticeship_in_the_application()
        {
            //Arrange
            const int numberOfApprenticeships = 3;
            var incentiveApplication = _fixture.Create<IncentiveApplication>();
            incentiveApplication.SetApprenticeships(_fixture.CreateMany<Apprenticeship>(numberOfApprenticeships).ToList());
            var apprenticeships = _fixture.CreateMany<CreateIncentiveCommand.IncentiveApprenticeship>().ToList();

            var itemsPersisted = 0;
            var command = new CreateIncentiveCommand(incentiveApplication.AccountId, incentiveApplication.AccountLegalEntityId, apprenticeships);

            _mockIncentiveDomainRepository.Setup(m => m.Save(It.IsAny<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>()))
                .Callback(() =>
                {
                    itemsPersisted++;
                });

            // Act
            await _sut.Handle(command);

            // Assert
            itemsPersisted.Should().Be(numberOfApprenticeships);
        }
    }
}
