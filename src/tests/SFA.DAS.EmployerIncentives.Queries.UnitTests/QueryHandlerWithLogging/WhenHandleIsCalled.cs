using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Queries.Decorators;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Queries.UnitTests.QueryHandlerWithLogging
{
    public class WhenHandleIsCalled
    {
        private QueryHandlerWithLogging<TestQuery, TestResult> _sut;
        private Mock<IQueryHandler<TestQuery, TestResult>> _mockHandler;
        private Mock<ILogger<TestQuery>> _mockLogger;
        private Fixture _fixture;

        public class TestQuery : IQuery { }
        public class TestResult { }

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _mockHandler = new Mock<IQueryHandler<TestQuery, TestResult>>();
            _mockLogger = new Mock<ILogger<TestQuery>>();

            _sut = new QueryHandlerWithLogging<TestQuery, TestResult>(_mockHandler.Object, _mockLogger.Object);
        }

        [Test]
        public async Task Then_the_start_of_the_call_is_logged()
        {
            // Arrange
            var query = new TestQuery();

            // Act
            await _sut.Handle(query);

            // Assert
            _mockLogger.VerifyLog(LogLevel.Debug, Times.Once(), $"Start handle '{typeof(TestQuery)}' query");
        }

        [Test]
        public async Task Then_the_end_of_the_call_is_logged()
        {
            // Arrange
            var query = new TestQuery();

            // Act
            await _sut.Handle(query);

            // Assert
            _mockLogger.VerifyLog(LogLevel.Debug, Times.Once(), $"End handle '{typeof(TestQuery)}' query");
        }

        [Test]
        public async Task Then_the_Query_is_passed_to_the_handler()
        {
            // Arrange
            var query = new TestQuery();

            // Act
            await _sut.Handle(query);

            // Assert
            _mockHandler.Verify(m => m.Handle(query, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void Then_a_handler_exception_is_logged()
        {
            // Arrange
            var query = new TestQuery();
            var exception = new Exception();

            _mockHandler
                .Setup(m => m.Handle(query, It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            // Act
            Func<Task> action = async () => await _sut.Handle(query);
            action.Invoke();

            // Assert
            _mockLogger.VerifyLog(LogLevel.Error, Times.Once(), $"Error handling '{typeof(TestQuery)}' query", exception);
        }

        [Test]
        public void Then_a_handler_exception_is_propogated()
        {
            // Arrange
            var query = new TestQuery();
            var errorMessage = _fixture.Create<string>();

            _mockHandler
                .Setup(m => m.Handle(query, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(errorMessage));

            // Act
            Func<Task> action = async () => await _sut.Handle(query);

            // Assert
            action.Should().Throw<Exception>().WithMessage(errorMessage);
        }
    }
}
