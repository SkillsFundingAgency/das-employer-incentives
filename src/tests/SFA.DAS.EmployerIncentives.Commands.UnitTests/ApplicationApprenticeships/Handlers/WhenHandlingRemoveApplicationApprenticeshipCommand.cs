using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.ApplicationApprenticeships;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApplicationApprenticeships.Handlers
{
    [TestFixture]
    public class WhenHandlingRemoveApplicationApprenticeshipCommand
    {
        private RemoveApplicationApprenticeshipCommandHandler _sut;
        private Mock<IIncentiveApplicationDomainRepository> _repository;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize(new IncentiveApplicationCustomization());

            _repository = new Mock<IIncentiveApplicationDomainRepository>();

            _sut = new RemoveApplicationApprenticeshipCommandHandler(_repository.Object);
        }

        [Test]
        public async Task Then_the_apprenticeship_is_removed_from_the_application()
        {
            // Arrange
            var incentiveApplication = _fixture.Create<IncentiveApplication>();
            var command = new RemoveApplicationApprenticeshipCommand(incentiveApplication.Id, _fixture.Create<long>());

            _repository.Setup(x => x.Find(command.IncentiveApplicationId)).ReturnsAsync(incentiveApplication);

            // Act
            await _sut.Handle(command);

            // Assert
            _repository.Verify(m => m.Save(incentiveApplication), Times.Once);
        }

        [Test]
        public async Task Then_the_apprenticeship_is_not_removed_if_the_application_does_not_exist()
        {
            // Arrange
            var command = _fixture.Create<RemoveApplicationApprenticeshipCommand>();

            IncentiveApplication nullApplication = null;
            _repository.Setup(x => x.Find(command.IncentiveApplicationId)).ReturnsAsync(nullApplication);

            // Act
            Func<Task> action = async () => await _sut.Handle(command);

            // Assert
            action.Should().Throw<InvalidRequestException>();
            _repository.Verify(m => m.Save(It.IsAny<IncentiveApplication>()), Times.Never);
        }
    }
}
