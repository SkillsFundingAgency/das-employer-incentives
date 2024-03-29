﻿using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Decorators;
using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;

namespace SFA.DAS.EmployerIncentives.Application.UnitTests.Commands.CommandHandlerWithLogging
{
    public class WhenHandleIsCalled
    {
        private CommandHandlerWithLogging<TestCommand> _sut;
        private Mock<ICommandHandler<TestCommand>> _mockHandler;
        private Mock<ILogger<TestCommand>> _mockLogger;
        private ILoggerFactory _loggerFactory;
        private Mock<ILoggerProvider> _mockLoggerProvider;
        private Fixture _fixture;

        public class TestCommand : ICommand { }

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _mockHandler = new Mock<ICommandHandler<TestCommand>>();
            _mockLogger = new Mock<ILogger<TestCommand>>();

            _mockLoggerProvider = new Mock<ILoggerProvider>();
            _mockLoggerProvider.Setup(m => m.CreateLogger(It.IsAny<string>())).Returns(_mockLogger.Object);
            
            _loggerFactory = new LoggerFactory();
            _loggerFactory.AddProvider(_mockLoggerProvider.Object);

            _sut = new CommandHandlerWithLogging<TestCommand>(_mockHandler.Object, _loggerFactory);
        }

        [Test]
        public async Task Then_the_start_of_the_call_is_logged()
        {
            //Arrange
            var command = new TestCommand();

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLogger.VerifyLog(LogLevel.Debug, Times.Once(), $"Start handle '{typeof(TestCommand)}' command : ");
        }

        [Test]
        public async Task Then_the_end_of_the_call_is_logged()
        {
            //Arrange
            var command = new TestCommand();

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLogger.VerifyLog(LogLevel.Debug, Times.Once(), $"End handle '{typeof(TestCommand)}' command : ");
        }

        [Test]
        public async Task Then_the_command_is_passed_to_the_handler()
        {
            //Arrange
            var command = new TestCommand();

            //Act
            await _sut.Handle(command);

            //Assert
            _mockHandler.Verify(m => m.Handle(command, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void Then_a_handler_exception_is_logged()
        {
            //Arrange
            var command = new TestCommand();
            var exception = new Exception();

            _mockHandler
                .Setup(m => m.Handle(command, It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            //Act
            Func<Task> action = async () => await _sut.Handle(command);
            action.Invoke();

            //Assert
            _mockLogger.VerifyLog(LogLevel.Error, Times.Once(), $"Error handling '{typeof(TestCommand)}' command : ", exception);
        }

        [Test]
        public void Then_a_handler_exception_is_propogated()
        {
            //Arrange
            var command = new TestCommand();
            var errorMessage = _fixture.Create<string>();

            _mockHandler
                .Setup(m => m.Handle(command, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(errorMessage));

            //Act
            Func<Task> action = async () => await _sut.Handle(command);

            //Assert
            action.Should().Throw<Exception>().WithMessage(errorMessage);
        }
    }
}
