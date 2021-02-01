using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.CreateIncentiveApplication;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.CreateIncentiveApplication.Handlers
{
    public class WhenHandlingCreateIncentiveApplicationCommand
    {
        private CreateIncentiveApplicationCommandHandler _sut;
        private Mock<IIncentiveApplicationFactory> _mockDomainFactory;
        private Mock<IIncentiveApplicationDomainRepository> _mockDomainRespository;
         
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize(new IncentiveApplicationCustomization());

            _mockDomainFactory = new Mock<IIncentiveApplicationFactory>();
            _mockDomainRespository = new Mock<IIncentiveApplicationDomainRepository>();
            
            _sut = new CreateIncentiveApplicationCommandHandler(_mockDomainFactory.Object, _mockDomainRespository.Object);
        }

        [Test]
        public async Task Then_the_a_new_application_is_persisted_to_the_domain_repository()
        {
            //Arrange
            var command = _fixture.Create<CreateIncentiveApplicationCommand>();
            var incentiveApplication = _fixture.Create<IncentiveApplication>();

            _mockDomainFactory.Setup(x => x.CreateNew(command.IncentiveApplicationId, command.AccountId, command.AccountLegalEntityId)).Returns(incentiveApplication);
            foreach (var apprenticeship in command.Apprenticeships)
            {
                _mockDomainFactory.Setup(x => x.CreateApprenticeship(
                    apprenticeship.ApprenticeshipId, apprenticeship.FirstName, apprenticeship.LastName,
                    apprenticeship.DateOfBirth, apprenticeship.ULN, apprenticeship.PlannedStartDate,
                    apprenticeship.ApprenticeshipEmployerTypeOnApproval, apprenticeship.UKPRN.Value, apprenticeship.CourseName)).Returns(_fixture.Create<Apprenticeship>());
            }
            
            //Act
            await _sut.Handle(command);

            //Assert
            incentiveApplication.Apprenticeships.Count.Should().Be(command.Apprenticeships.Count());
            _mockDomainRespository.Verify(m => m.Save(incentiveApplication), Times.Once);
        }
    }
}
