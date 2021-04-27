using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.ApplicationApprenticeships;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Application
{
    [TestFixture]
    public class WhenAddingApprenticeshipsToApplication
    {
        private ApplicationCommandController _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            _fixture = new Fixture();
            _sut = new ApplicationCommandController(_mockCommandDispatcher.Object);

            _mockCommandDispatcher
                .Setup(m => m.Send(It.IsAny<AddApplicationApprenticeshipsCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Test]
        public async Task Then_an_add_apprenticeship_to_application_command_is_dispatched()
        {
            // Arrange
            var request = _fixture.Create<AddApplicationApprenticeshipsRequest>();

            // Act
            await _sut.AddApprenticeshipsToApplication(request);

            // Assert
            _mockCommandDispatcher
                .Verify(m => m.Send(It.Is<AddApplicationApprenticeshipsCommand>(c =>
                            c.IncentiveApplicationId == request.IncentiveApplicationId &&
                            c.AccountId == request.AccountId &&
                            c.Apprenticeships == request.Apprenticeships),
                        It.IsAny<CancellationToken>())
                    , Times.Once);
        }

        [Test]
        public async Task Then_an_Ok_response_is_returned()
        {
            // Arrange
            var request = _fixture.Create<AddApplicationApprenticeshipsRequest>();

            // Act
            var actual = await _sut.AddApprenticeshipsToApplication(request) as OkResult;

            // Assert
            actual.Should().NotBeNull();
        }
    }
}
