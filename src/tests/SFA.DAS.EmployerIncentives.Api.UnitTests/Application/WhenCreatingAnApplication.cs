using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.CreateIncentiveApplication;
using SFA.DAS.EmployerIncentives.Commands.UpsertLegalEntity;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Application
{
    public class WhenCreatingAnApplication
    {
        private ApplicationCommandController _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;
        private Mock<ILogger<ApplicationCommandController>> _mockLogger;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            _mockLogger = new Mock<ILogger<ApplicationCommandController>>();
            _fixture = new Fixture();
            _sut = new ApplicationCommandController(_mockCommandDispatcher.Object, _mockLogger.Object);

            _mockCommandDispatcher
                .Setup(m => m.Send(It.IsAny<UpsertLegalEntityCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Test]
        public async Task Then_a_CreateIncentiveApplicationCommand_command_is_dispatched()
        {
            // Arrange
            var request = _fixture.Create<CreateIncentiveApplicationRequest>();

            // Act
            await _sut.CreateIncentiveApplication(request);

            // Assert
            _mockCommandDispatcher
                .Verify(m => m.Send(It.Is<CreateIncentiveApplicationCommand>(c => 
                    c.IncentiveApplicationId == request.IncentiveApplicationId &&
                    c.AccountId == request.AccountId && 
                    c.AccountLegalEntityId == request.AccountLegalEntityId &&
                    c.Apprenticeships == request.Apprenticeships), 
                It.IsAny<CancellationToken>())
                ,Times.Once);                
        }

        [Test]
        public async Task Then_a_created_response_is_returned()
        {
            // Arrange
            var request = _fixture.Create<CreateIncentiveApplicationRequest>();
            
            // Act
            var actual = await _sut.CreateIncentiveApplication(request) as CreatedResult;

            // Assert
            actual.Should().NotBeNull();
        }
    }
}