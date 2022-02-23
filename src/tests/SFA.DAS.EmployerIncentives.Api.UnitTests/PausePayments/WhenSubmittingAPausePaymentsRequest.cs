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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
                .Setup(m => m.SendMany(It.IsAny<IEnumerable<PausePaymentsCommand>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Test]
        public async Task Then_the_PausePaymentsCommand_is_dispatched()
        {
            // Arrange
            var request = _fixture.Create<PausePaymentsRequest>();
            request.Applications = _fixture.CreateMany<Types.Application>(1).ToArray();

            PausePaymentsCommand sentCommand = null;

            _mockCommandDispatcher
                .Setup(m => m.SendMany(It.IsAny<IEnumerable<ICommand>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<ICommand>, CancellationToken>( (c, t) =>
                {
                    sentCommand = c.Single() as PausePaymentsCommand;
                });

            // Act
            await _sut.PausePayments(request);

            // Assert
            _mockCommandDispatcher
                .Verify(m => m.SendMany(It.Is<List<ICommand>>(c => c.Count == 1), 
                It.IsAny<CancellationToken>())
                ,Times.Once);
            
            sentCommand.AccountLegalEntityId.Should().Be(request.Applications.Single().AccountLegalEntityId);
            sentCommand.ULN.Should().Be(request.Applications.Single().ULN);
            sentCommand.ServiceRequestId.Should().Be(request.ServiceRequest.TaskId);
            sentCommand.DateServiceRequestTaskCreated.Should().Be(request.ServiceRequest.TaskCreatedDate.Value);
            sentCommand.DecisionReferenceNumber.Should().Be(request.ServiceRequest.DecisionReference);
            sentCommand.Action.Should().Be(request.Action);
        }

        [Test]
        public async Task Then_multiple_PausePaymentsCommands_are_dispatched()
        {
            // Arrange
            var request = _fixture.Create<PausePaymentsRequest>();
            request.Applications = _fixture.CreateMany<Types.Application>(2).ToArray();

            PausePaymentsCommand sentCommand1 = null;
            PausePaymentsCommand sentCommand2 = null;

            _mockCommandDispatcher
                .Setup(m => m.SendMany(It.IsAny<IEnumerable<ICommand>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<ICommand>, CancellationToken>((c, t) =>
                {
                    sentCommand1 = c.First() as PausePaymentsCommand;
                    sentCommand2 = c.Last() as PausePaymentsCommand;
                });

            // Act
            await _sut.PausePayments(request);

            // Assert
            _mockCommandDispatcher
                .Verify(m => m.SendMany(It.Is<List<ICommand>>(c => c.Count == 2),
                It.IsAny<CancellationToken>())
                , Times.Once);

            sentCommand1.AccountLegalEntityId.Should().Be(request.Applications.First().AccountLegalEntityId);
            sentCommand1.ULN.Should().Be(request.Applications.First().ULN);
            sentCommand1.ServiceRequestId.Should().Be(request.ServiceRequest.TaskId);
            sentCommand1.DateServiceRequestTaskCreated.Should().Be(request.ServiceRequest.TaskCreatedDate.Value);
            sentCommand1.DecisionReferenceNumber.Should().Be(request.ServiceRequest.DecisionReference);
            sentCommand1.Action.Should().Be(request.Action);

            sentCommand2.AccountLegalEntityId.Should().Be(request.Applications.Last().AccountLegalEntityId);
            sentCommand2.ULN.Should().Be(request.Applications.Last().ULN);
            sentCommand2.ServiceRequestId.Should().Be(request.ServiceRequest.TaskId);
            sentCommand2.DateServiceRequestTaskCreated.Should().Be(request.ServiceRequest.TaskCreatedDate.Value);
            sentCommand2.DecisionReferenceNumber.Should().Be(request.ServiceRequest.DecisionReference);
            sentCommand2.Action.Should().Be(request.Action);
        }

        [Test]
        public async Task Then_an_ok_response_is_returned()
        {
            // Arrange
            var request = _fixture.Create<PausePaymentsRequest>();

            // Act
            var actual = await _sut.PausePayments(request) as OkObjectResult;

            // Assert
            actual.Should().NotBeNull();
        }

        [Test]
        public async Task Then_when_no_matching_record_a_not_found_response_is_returned()
        {
            // Arrange
            var request = _fixture.Create<PausePaymentsRequest>();
            _mockCommandDispatcher.Setup(x => x.SendMany(It.IsAny<IEnumerable<ICommand>>(), It.IsAny<CancellationToken>()))
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
            var request = _fixture.Create<PausePaymentsRequest>();
            _mockCommandDispatcher.Setup(x => x.SendMany(It.IsAny<IEnumerable<ICommand>>(), It.IsAny<CancellationToken>()))
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
            var request = _fixture.Create<PausePaymentsRequest>();

            _mockCommandDispatcher
                .Setup(m => m.SendMany(It.IsAny<IEnumerable<ICommand>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new PausePaymentsException(""));

            // Act
            var actual = await _sut.PausePayments(request) as BadRequestObjectResult;

            // Assert
            actual.Should().NotBeNull();
        }

        [Test]
        public async Task Then_when_an_Exception_is_thrown_a_bad_request_response_is_returned()
        {
            // Arrange
            var request = _fixture.Create<PausePaymentsRequest>();

            _mockCommandDispatcher
                .Setup(m => m.SendMany(It.IsAny<IEnumerable<ICommand>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new System.Exception(""));

            // Act
            var actual = await _sut.PausePayments(request) as BadRequestResult;

            // Assert
            actual.Should().NotBeNull();
        }
    }
}