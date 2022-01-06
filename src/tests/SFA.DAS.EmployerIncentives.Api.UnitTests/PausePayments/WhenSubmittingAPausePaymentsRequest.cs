using System.Collections.Generic;
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
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.PausePayments;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using SFA.DAS.EmployerIncentives.Commands.UpsertLegalEntity;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.PausePayments
{
    public class WhenSubmittingAPausePaymentsRequest
    {
        private PausePaymentsController _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            _fixture = new Fixture();
            _sut = new PausePaymentsController(_mockCommandDispatcher.Object);

            _mockCommandDispatcher
                .Setup(m => m.Send(It.IsAny<UpsertLegalEntityCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Test]
        public async Task Then_the_PausePaymentsCommand_is_dispatched()
        {
            // Arrange
            var request = _fixture.Create<PausePayment>();

            // Act
            await _sut.PausePayments(request);

            // Assert
            _mockCommandDispatcher
                .Verify(m => m.Send(It.Is<PausePaymentsCommand>(c => 
                    c.AccountLegalEntityId == request.AccountLegalEntityId &&
                    c.ULN == request.ULN && 
                    c.ServiceRequestId == request.ServiceRequest.TaskId &&
                    c.DateServiceRequestTaskCreated == request.ServiceRequest.TaskCreatedDate.Value &&
                    c.DecisionReferenceNumber == request.ServiceRequest.DecisionReference), 
                It.IsAny<CancellationToken>())
                ,Times.Once);                
        }

        [Test]
        public async Task Then_an_ok_response_is_returned()
        {
            // Arrange
            var request = _fixture.Create<PausePayment>();

            // Act
            var actual = await _sut.PausePayments(request) as OkObjectResult;

            // Assert
            actual.Should().NotBeNull();
        }

        [Test]
        public async Task Then_when_no_matching_record_a_not_found_response_is_returned()
        {
            // Arrange
            var request = _fixture.Create<PausePayment>();
            _mockCommandDispatcher.Setup(x => x.Send(It.IsAny<ICommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException(""));

            // Act
            var actual = await _sut.PausePayments(request) as NotFoundObjectResult;

            // Assert
            actual.Should().NotBeNull();
        }

        [Test]
        public async Task Then_when_request_is_not_valid_a_bad_request_response_is_returned()
        {
            // Arrange
            var request = _fixture.Create<PausePayment>();
            _mockCommandDispatcher.Setup(x => x.Send(It.IsAny<ICommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidRequestException());

            // Act
            var actual = await _sut.PausePayments(request) as BadRequestObjectResult;

            // Assert
            actual.Should().NotBeNull();
        }

        [Test]
        public async Task Then_when_a_PausePaymentsException_is_thrown_a_bad_request_response_is_returned()
        {
            // Arrange
            var request = _fixture.Create<PausePayment>();
            _mockCommandDispatcher.Setup(x => x.Send(It.IsAny<ICommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new PausePaymentsException(""));

            // Act
            var actual = await _sut.PausePayments(request) as BadRequestObjectResult;

            // Assert
            actual.Should().NotBeNull();
        }
    }
}