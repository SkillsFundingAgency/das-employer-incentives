using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Commands.ApplicationApprenticeships;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Application
{
    [TestFixture]
    public class WhenRemovingApprenticeshipsFromApplication
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
                .Setup(m => m.Send(It.IsAny<RemoveApplicationApprenticeshipCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Test]
        public async Task Then_a_remove_apprenticeship_from_application_command_is_dispatched()
        {
            // Arrange
            var applicationId = Guid.NewGuid();
            var apprenticeshipId = _fixture.Create<long>();

            // Act
            await _sut.RemoveApprenticeshipsFromApplication(applicationId, apprenticeshipId);

            // Assert
            _mockCommandDispatcher
                .Verify(m => m.Send(It.Is<RemoveApplicationApprenticeshipCommand>(c =>
                            c.IncentiveApplicationId == applicationId &&
                            c.ApprenticeshipId == apprenticeshipId),
                        It.IsAny<CancellationToken>())
                    , Times.Once);
        }

        [Test]
        public async Task Then_an_Ok_response_is_returned()
        {
            // Arrange
            var applicationId = Guid.NewGuid();
            var apprenticeshipId = _fixture.Create<long>();

            // Act
            var actual = await _sut.RemoveApprenticeshipsFromApplication(applicationId, apprenticeshipId) as OkResult;

            // Assert
            actual.Should().NotBeNull();
        }
    }
}
