using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.UpdateIncentiveApplication;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Application
{
    public class WhenUpdatingAnApplication
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
        }

        [Test]
        public async Task Then_a_UpdateIncentiveApplicationCommand_command_is_dispatched()
        {
            // Arrange
            var request = _fixture.Create<UpdateIncentiveApplicationRequest>();

            // Act
            await _sut.UpdateIncentiveApplication(request);

            // Assert
            _mockCommandDispatcher
                .Verify(m => m.Send(It.Is<UpdateIncentiveApplicationCommand>(c =>
                    c.IncentiveApplicationId == request.IncentiveApplicationId &&
                    c.AccountId == request.AccountId &&
                    c.AccountLegalEntityId == request.AccountLegalEntityId &&
                    c.Apprenticeships == request.Apprenticeships),
                It.IsAny<CancellationToken>())
                , Times.Once);
        }

        [Test]
        public async Task Then_an_Ok_response_is_returned()
        {
            // Arrange
            var request = _fixture.Create<UpdateIncentiveApplicationRequest>();

            // Act
            var actual = await _sut.UpdateIncentiveApplication(request) as OkObjectResult;

            // Assert
            actual.Should().NotBeNull();
            actual.Value.Should().Be($"/applications/{request.IncentiveApplicationId}");
        }
    }
}