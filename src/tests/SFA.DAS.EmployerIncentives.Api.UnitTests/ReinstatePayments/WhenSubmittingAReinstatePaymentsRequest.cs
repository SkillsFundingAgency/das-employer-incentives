using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.PausePayments;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.ReinstatePayments;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.ReinstatePayments
{
    [TestFixture]
    public class WhenSubmittingAReinstatePaymentsRequest
    {
        private ReinstatePaymentsController _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            _fixture = new Fixture();
            _sut = new ReinstatePaymentsController(_mockCommandDispatcher.Object);

            _mockCommandDispatcher
                .Setup(m => m.SendMany(It.IsAny<IEnumerable<PausePaymentsCommand>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Test]
        public async Task Then_the_ReinstatePaymentsCommand_is_dispatched()
        {
            // Arrange
            var request = _fixture.Create<ReinstatePaymentsRequest>();
            request.Payments = _fixture.CreateMany<Guid>(1).ToList();

            ReinstatePendingPaymentCommand sentCommand = null;

            _mockCommandDispatcher
                .Setup(m => m.SendMany(It.IsAny<IEnumerable<ICommand>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<ICommand>, CancellationToken>( (c, t) =>
                {
                    sentCommand = c.Single() as ReinstatePendingPaymentCommand;
                });

            // Act
            await _sut.ReinstatePayments(request);

            // Assert
            _mockCommandDispatcher
                .Verify(m => m.SendMany(It.Is<List<ICommand>>(c => c.Count == 1), 
                It.IsAny<CancellationToken>())
                ,Times.Once);

            sentCommand.PendingPaymentId.Should().Be(request.Payments[0]);
            sentCommand.ReinstatePaymentRequest.TaskId.Should().Be(request.ServiceRequest.TaskId);
            sentCommand.ReinstatePaymentRequest.DecisionReference.Should().Be(request.ServiceRequest.DecisionReference);
            sentCommand.ReinstatePaymentRequest.Created.Should().Be(request.ServiceRequest.TaskCreatedDate);
            sentCommand.ReinstatePaymentRequest.Process.Should().Be(request.ServiceRequest.Process);
        }

        [Test]
        public async Task Then_multiple_ReinstatePaymentsCommands_are_dispatched()
        {
            // Arrange
            var request = _fixture.Create<ReinstatePaymentsRequest>();
            request.Payments = _fixture.CreateMany<Guid>(2).ToList();

            ReinstatePendingPaymentCommand sentCommand1 = null;
            ReinstatePendingPaymentCommand sentCommand2 = null;

            _mockCommandDispatcher
                .Setup(m => m.SendMany(It.IsAny<IEnumerable<ICommand>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<ICommand>, CancellationToken>((c, t) =>
                {
                    sentCommand1 = c.First() as ReinstatePendingPaymentCommand;
                    sentCommand2 = c.Last() as ReinstatePendingPaymentCommand;
                });

            // Act
            await _sut.ReinstatePayments(request);

            // Assert
            _mockCommandDispatcher
                .Verify(m => m.SendMany(It.Is<List<ICommand>>(c => c.Count == 2),
                It.IsAny<CancellationToken>())
                , Times.Once);

            sentCommand1.PendingPaymentId.Should().Be(request.Payments[0]);
            sentCommand1.ReinstatePaymentRequest.TaskId.Should().Be(request.ServiceRequest.TaskId);
            sentCommand1.ReinstatePaymentRequest.DecisionReference.Should().Be(request.ServiceRequest.DecisionReference);
            sentCommand1.ReinstatePaymentRequest.Created.Should().Be(request.ServiceRequest.TaskCreatedDate);
            sentCommand1.ReinstatePaymentRequest.Process.Should().Be(request.ServiceRequest.Process);

            sentCommand2.PendingPaymentId.Should().Be(request.Payments[1]);
            sentCommand2.ReinstatePaymentRequest.TaskId.Should().Be(request.ServiceRequest.TaskId);
            sentCommand2.ReinstatePaymentRequest.DecisionReference.Should().Be(request.ServiceRequest.DecisionReference);
            sentCommand2.ReinstatePaymentRequest.Created.Should().Be(request.ServiceRequest.TaskCreatedDate);
            sentCommand2.ReinstatePaymentRequest.Process.Should().Be(request.ServiceRequest.Process); }

        [Test]
        public async Task Then_an_ok_response_is_returned()
        {
            // Arrange
            var request = _fixture.Create<ReinstatePaymentsRequest>();

            // Act
            var actual = await _sut.ReinstatePayments(request) as OkObjectResult;

            // Assert
            actual.Should().NotBeNull();
        }

        [Test]
        public async Task Then_when_request_is_not_valid_a_bad_request_response_is_returned()
        {
            // Arrange
            var request = _fixture.Create<ReinstatePaymentsRequest>();
            const string errorMessage = "Validation error";
            _mockCommandDispatcher.Setup(x => x.SendMany(It.IsAny<IEnumerable<ICommand>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException(errorMessage));

            // Act
            var actual = await _sut.ReinstatePayments(request) as BadRequestObjectResult;

            // Assert
            actual.Should().NotBeNull();
            actual.Value.Should().Be(errorMessage);
        }
    }
}