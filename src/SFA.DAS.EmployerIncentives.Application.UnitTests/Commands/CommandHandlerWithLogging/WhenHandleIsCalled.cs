using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Application.Commands;
using SFA.DAS.EmployerIncentives.Application.Decorators;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Application.UnitTests.Commands.CommandHandlerWithLogging
{    
    public class WhenHandleIsCalled
    {
        private CommandHandlerWithLogging<TestCommand> _sut;
        private Mock<ICommandHandler<TestCommand>> _mockHandler;
        private Mock<ILogger<TestCommand>> _mockLogger;
        private Fixture _fixture;

        public class TestCommand : ICommand{}

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _mockHandler = new Mock<ICommandHandler<TestCommand>>();
            _mockLogger = new Mock<ILogger<TestCommand>>();

            _sut = new CommandHandlerWithLogging<TestCommand>(_mockHandler.Object, _mockLogger.Object);
        }

        [Test]
        public async Task Then_the_start_of_the_call_is_logged()
        {
            //Arrange
            var command = new TestCommand();

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLogger.VerifyLog(LogLevel.Information, Times.Once(), $"Start handle '{typeof(TestCommand)}' command");
        }

        [Test]
        public async Task Then_the_end_of_the_call_is_logged()
        {
            //Arrange
            var command = new TestCommand();

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLogger.VerifyLog(LogLevel.Information, Times.Once(), $"End handle '{typeof(TestCommand)}' command");
        }

        [Test]
        public async Task Then_the_command_is_passed_to_the_handler()
        {
            //Arrange
            var command = new TestCommand();

            //Act
            await _sut.Handle(command);

            //Assert
            _mockHandler.Verify(m => m.Handle(command), Times.Once);
        }

        [Test]
        public void Then_a_handler_exception_is_logged()
        {
            //Arrange
            var command = new TestCommand();
            var exception = new Exception();

            _mockHandler
                .Setup(m => m.Handle(command))
                .ThrowsAsync(exception);

            //Act
            Func<Task> action = async () => await _sut.Handle(command);
            action.Invoke();

            //Assert
            _mockLogger.VerifyLog(LogLevel.Error, Times.Once(), $"Error handling '{typeof(TestCommand)}' command", exception);
        }

        [Test]
        public void Then_a_handler_exception_is_propogated()
        {
            //Arrange
            var command = new TestCommand();
            var errorMessage = _fixture.Create<string>();

            _mockHandler
                .Setup(m => m.Handle(command))
                .ThrowsAsync(new Exception(errorMessage));

            //Act
            Func<Task> action = async () => await _sut.Handle(command);

            //Assert
            action.Should().Throw<Exception>().WithMessage(errorMessage);
        }
    }
}
