using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.EmploymentCheck
{
    public class WhenUpdatingEmploymentCheck
    {
        private EmploymentCheckController _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            _fixture = new Fixture();
            _sut = new EmploymentCheckController(_mockCommandDispatcher.Object);
        }

        [Test]
        public async Task Then_a_UpdateEmploymentCheckCommand_command_is_dispatched()
        {
            // Arrange
            var request = _fixture.Build<UpdateEmploymentCheckRequest>()
                 .With(e => e.Result, EmploymentCheckResultType.Employed.ToString())
                 .Create();

            // Act
            await _sut.Update(request);

            // Assert
            _mockCommandDispatcher
                .Verify(m => m.Send(It.Is<UpdateEmploymentCheckCommand>(c =>
                    c.CorrelationId == request.CorrelationId &&
                    c.Result == Map(request.Result) &&
                    c.DateChecked == request.DateChecked),
                It.IsAny<CancellationToken>())
                , Times.Once);
        }

        [TestCase("Employed")]
        [TestCase("NotEmployed")]
        public async Task Then_an_Ok_response_is_returned(string sucessResult)
        {
            // Arrange
            var request = _fixture.Build<UpdateEmploymentCheckRequest>()
                .With(e => e.Result, sucessResult)
                .Create();

            // Act
            var actual = await _sut.Update(request) as OkObjectResult;

            // Assert
            actual.Should().NotBeNull();
            actual.Value.Should().Be($"/employmentchecks/{request.CorrelationId}");
        }

        [TestCase(EmploymentCheckResultError.HmrcFailure)]
        [TestCase(EmploymentCheckResultError.NinoAndPAYENotFound)]
        [TestCase(EmploymentCheckResultError.NinoFailure)]
        [TestCase(EmploymentCheckResultError.NinoInvalid)]
        [TestCase(EmploymentCheckResultError.NinoNotFound)]
        [TestCase(EmploymentCheckResultError.PAYEFailure)]
        [TestCase(EmploymentCheckResultError.PAYENotFound)]
        public async Task Then_an_Ok_response_is_returned_with_error_types(EmploymentCheckResultError errorType)
        {
            // Arrange
            var request = _fixture.Build<UpdateEmploymentCheckRequest>()
                .With(e => e.Result, errorType.ToString)
                .Create();

            // Act
            var actual = await _sut.Update(request) as OkObjectResult;

            // Assert
            actual.Should().NotBeNull();
            actual.Value.Should().Be($"/employmentchecks/{request.CorrelationId}");
        }

        [Test]
        public async Task Then_a_BadRequest_response_is_returned_when_an_InvalidEmploymentCheckErrorTypeException_is_thrown()
        {
            // Arrange
            var request = _fixture.Build<UpdateEmploymentCheckRequest>()
                 .With(e => e.Result, EmploymentCheckResultType.Employed.ToString())
                 .Create();

            _mockCommandDispatcher.Setup(m => m.Send(It.IsAny<UpdateEmploymentCheckCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidEmploymentCheckErrorTypeException());

            _sut = new EmploymentCheckController(_mockCommandDispatcher.Object);

            // Act
            var actual = await _sut.Update(request) as BadRequestResult;

            // Assert
            actual.Should().NotBeNull();
        }

        [Test]
        public async Task Then_a_BadRequest_response_is_returned_when_an_InvalidEmploymentCheckErrorType_is_passed()
        {
            // Arrange
            var request = _fixture.Build<UpdateEmploymentCheckRequest>()
                 .With(e => e.Result, Guid.NewGuid().ToString())
                 .Create();
            
            _sut = new EmploymentCheckController(_mockCommandDispatcher.Object);

            // Act
            var actual = await _sut.Update(request) as BadRequestResult;

            // Assert
            actual.Should().NotBeNull();
        }

        private EmploymentCheckResultType Map(string result)
        {
            return result.ToLower() switch
            {
                "employed" => EmploymentCheckResultType.Employed,
                "notemployed" => EmploymentCheckResultType.NotEmployed,                
                _ => EmploymentCheckResultType.Error,
            };
        }
    }
}
