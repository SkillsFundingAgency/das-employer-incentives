using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Decorators;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Application.UnitTests.Commands.CommandHandlerWithValidator
{
    public class WhenHandleIsCalled
    {
        private CommandHandlerWithValidator<TestCommand> _sut;
        private Mock<ICommandHandler<TestCommand>> _mockHandler;
        private Mock<IValidator<TestCommand>> _mockValidator;
        private Fixture _fixture;

        public class TestCommand : ICommand{}

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _mockHandler = new Mock<ICommandHandler<TestCommand>>();
            _mockValidator = new Mock<IValidator<TestCommand>>();
            _mockValidator
               .Setup(m => m.Validate(It.IsAny<TestCommand>()))
               .ReturnsAsync(new ValidationResult());

            _sut = new CommandHandlerWithValidator<TestCommand>(_mockHandler.Object, _mockValidator.Object);
        }

        [Test]
        public async Task Then_the_command_is_validated()
        {
            //Arrange
            var command = _fixture.Create<TestCommand>();

            //Act
            await _sut.Handle(command);

            //Assert
            _mockValidator.Verify(m => m.Validate(command), Times.Once);
        }

        [Test]
        public Task Then_an_InvalidRequestException_is_raised_when_the_validator_returns_a_null_result()
        {
            //Arrange
            var command = _fixture.Create<TestCommand>();

            var validationResult = new ValidationResult();
            validationResult.AddError(_fixture.Create<string>());
            _mockValidator
               .Setup(m => m.Validate(It.IsAny<TestCommand>()))
               .ReturnsAsync(null as ValidationResult);

            //Act
            Func<Task> action = async () => await _sut.Handle(command);

            //Assert
            return action.Should().ThrowAsync<InvalidRequestException>();
        }

        [Test]
        public Task Then_an_InvalidRequestException_is_raised_when_the_command_is_not_valid()
        {
            //Arrange
            var command = _fixture.Create<TestCommand>();

            var validationResult = new ValidationResult();
            validationResult.AddError(_fixture.Create<string>());
            _mockValidator
               .Setup(m => m.Validate(It.IsAny<TestCommand>()))
               .ReturnsAsync(validationResult);

            //Act
            Func<Task> action = async () => await _sut.Handle(command);

            //Assert
            return action.Should().ThrowAsync<InvalidRequestException>();
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
    }
}
