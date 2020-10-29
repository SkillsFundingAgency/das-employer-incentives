using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Command
{
    public class WhenRunningACommand
    {
        private CommandController _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            _fixture = new Fixture();
            _sut = new CommandController(_mockCommandDispatcher.Object);

            _mockCommandDispatcher
                .Setup(m => m.Send(It.IsAny<DomainCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Test]
        public async Task Then_a_DomainCommand_command_is_dispatched()
        {
            // Arrange
            var command = _fixture.Create<CreateIncentiveCommand>();
            var commandText = JsonConvert.SerializeObject(command, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            // Act
            await _sut.RunCommand("ApprenticeshipIncentive.CreateIncentiveCommand", commandText);

            // Assert
            _mockCommandDispatcher
                .Verify(m => m.Send(It.Is<CreateIncentiveCommand>(c =>
                    c.AccountId == command.AccountId &&
                    c.AccountLegalEntityId == command.AccountLegalEntityId &&
                    c.Apprenticeships.Count == command.Apprenticeships.Count
                    ),
                It.IsAny<CancellationToken>())
                , Times.Once);
        }

        [Test]
        public async Task Then_an_OK_result_is_returned_on_dispatch()
        {
            // Arrange
            var command = _fixture.Create<CreateIncentiveCommand>();
            var commandText = JsonConvert.SerializeObject(command, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            // Act
            var response = await _sut.RunCommand("ApprenticeshipIncentive.CreateIncentiveCommand", commandText) as OkResult;

            // Assert
            response.StatusCode.Should().Be(200);
        }

        [Test]
        public async Task Then_a_command_is_not_dispatched_when_not_a_domain_command()
        {
            // Arrange
            var command = _fixture.Create<TestCommand>();
            var commandText = JsonConvert.SerializeObject(command, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            // Act
            await _sut.RunCommand("TestCommand", commandText);

            // Assert
            _mockCommandDispatcher
                .Verify(m => m.Send(It.IsAny<ICommand>(),
                It.IsAny<CancellationToken>())
                , Times.Never);
        }
    }

    public class TestCommand : DomainCommand
    {
        public string Test { get; set; }
    }
}
