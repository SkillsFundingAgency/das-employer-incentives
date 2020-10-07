using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CreateIncentive;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.Create.Handlers
{
    public class WhenHandlingCreateIncentiveCommand
    {
        private CreateIncentiveCommandHandler _sut;
        private Mock<IIncentiveApplicationDomainRepository> _mockApplicationDomainRespository;        
        private Mock<IApprenticeshipIncentiveDomainRepository> _mockIncentiveDomainRespository;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            
            _mockApplicationDomainRespository = new Mock<IIncentiveApplicationDomainRepository>();
            _mockIncentiveDomainRespository = new Mock<IApprenticeshipIncentiveDomainRepository>();

            _fixture.Customize(new ApprenticeshipIncentiveCustomization());
            _fixture.Customize(new IncentiveApplicationCustomization());
            _sut = new CreateIncentiveCommandHandler(
                _mockApplicationDomainRespository.Object, 
                new ApprenticeshipIncentiveFactory(),
                _mockIncentiveDomainRespository.Object);
        }

        [Test]
        public async Task Then_an_apprenticeship_incentive_created_event_is_raised_for_each_apprenticeship_in_the_application()
        {
            //Arrange
            int numberOfApprenticeships = 5;
            var createdIncentives = new List<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();
            var incentiveApplication = _fixture.Create<IncentiveApplication>();
            incentiveApplication.SetApprenticeships(_fixture.CreateMany<Domain.IncentiveApplications.Apprenticeship>(numberOfApprenticeships).ToList());

            var command = new CreateIncentiveCommand(incentiveApplication.AccountId, incentiveApplication.Id);

            _mockApplicationDomainRespository.Setup(x => x
            .Find(command.IncentiveApplicationId))
                .ReturnsAsync(incentiveApplication);

            _mockIncentiveDomainRespository
                .Setup(m => m.Save(It.IsAny<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>()))
                .Callback<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>((i) =>
                {
                    createdIncentives.Add(i);
                });

            // Act
            await _sut.Handle(command);

            // Assert
            createdIncentives.Count.Should().Be(numberOfApprenticeships);
            createdIncentives.ForEach(i => i.FlushEvents().OfType<Created>().ToList().Count.Should().Be(1));
        }

        [Test]
        public async Task Then_an_apprenticeship_incentive_is_persisted_for_each_apprenticeship_in_the_application()
        {
            //Arrange
            int numberOfApprenticeships = 3;
            var incentiveApplication = _fixture.Create<IncentiveApplication>();
            incentiveApplication.SetApprenticeships(_fixture.CreateMany<Domain.IncentiveApplications.Apprenticeship>(numberOfApprenticeships).ToList());

            int itemsPersisted = 0;
            var command = new CreateIncentiveCommand(incentiveApplication.AccountId, incentiveApplication.Id);

            _mockApplicationDomainRespository.Setup(x => x.Find(command.IncentiveApplicationId)).ReturnsAsync(incentiveApplication);

            _mockIncentiveDomainRespository.Setup(m => m.Save(It.IsAny<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>()))
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
