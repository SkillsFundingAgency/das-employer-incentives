using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApplicationApprenticeships;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApplicationApprenticeships.Handlers
{
    [TestFixture]
    public class WhenHandlingAddApplicationApprenticeshipsCommand
    {
        private AddApplicationApprenticeshipsCommandHandler _sut;
        private Mock<IIncentiveApplicationFactory> _domainFactory;
        private Mock<IIncentiveApplicationDomainRepository> _repository;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize(new IncentiveApplicationCustomization());

            _domainFactory = new Mock<IIncentiveApplicationFactory>();
            _repository = new Mock<IIncentiveApplicationDomainRepository>();

            _sut = new AddApplicationApprenticeshipsCommandHandler(_repository.Object, _domainFactory.Object);
        }

        [Test]
        public async Task Then_the_apprenticeships_are_added_to_the_application()
        {
            // Arrange
            var incentiveApplication = _fixture.Create<IncentiveApplication>();
            var command = new AddApplicationApprenticeshipsCommand(incentiveApplication.Id,
                incentiveApplication.AccountId, _fixture.CreateMany<IncentiveApplicationApprenticeshipDto>(5));

            _repository.Setup(x => x.Find(command.IncentiveApplicationId)).ReturnsAsync(incentiveApplication);

            foreach (var apprenticeship in command.Apprenticeships)
            {
                _domainFactory.Setup(x => x.CreateApprenticeship(
                    apprenticeship.ApprenticeshipId, apprenticeship.FirstName, apprenticeship.LastName,
                    apprenticeship.DateOfBirth, apprenticeship.ULN, apprenticeship.PlannedStartDate,
                    apprenticeship.ApprenticeshipEmployerTypeOnApproval, apprenticeship.UKPRN.Value,
                    apprenticeship.CourseName)).Returns(_fixture.Create<Apprenticeship>());
            }

            // Act
            await _sut.Handle(command);

            // Assert
            _repository.Verify(m => m.Save(incentiveApplication), Times.Once);
        }

        [Test]
        public async Task Then_the_apprenticeships_are_not_added_if_the_application_does_not_exist()
        {
            // Arrange
            var command = _fixture.Create<AddApplicationApprenticeshipsCommand>();

            IncentiveApplication nullApplication = null;
            _repository.Setup(x => x.Find(command.IncentiveApplicationId)).ReturnsAsync(nullApplication);

            // Act
            Func<Task> action = async () => await _sut.Handle(command);

            // Assert
            action.Should().Throw<InvalidRequestException>();
            _repository.Verify(m => m.Save(It.IsAny<IncentiveApplication>()), Times.Never);
        }

        [Test]
        public async Task Then_the_apprenticeships_are_not_added_if_the_application_is_not_for_the_specified_account()
        {
            // Arrange
            var incentiveApplication = _fixture.Create<IncentiveApplication>();
            var command = new AddApplicationApprenticeshipsCommand(incentiveApplication.Id,
                incentiveApplication.AccountId + 1, _fixture.CreateMany<IncentiveApplicationApprenticeshipDto>(5));

            _repository.Setup(x => x.Find(command.IncentiveApplicationId)).ReturnsAsync(incentiveApplication);

            // Act
            Func<Task> action = async () => await _sut.Handle(command);

            // Assert
            action.Should().Throw<InvalidRequestException>();
            _repository.Verify(m => m.Save(It.IsAny<IncentiveApplication>()), Times.Never);
        }
    }
}
