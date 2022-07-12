using System;
using System.Collections.Generic;
using System.Linq;
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
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.RevertPayments;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.RevertPayments
{
    [TestFixture]
    public class WhenSubmittingARevertPaymentsRequest
    {
        private RevertPaymentsController _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            _sut = new RevertPaymentsController(_mockCommandDispatcher.Object);

            _mockCommandDispatcher
                .Setup(m => m.SendMany(It.IsAny<IEnumerable<RevertPaymentCommand>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Test]
        public async Task Then_a_revert_payment_command_is_dispatched()
        {
            // Arrange
            var request = _fixture.Build<RevertPaymentsRequest>()
                .With(x => x.Payments, new List<Guid> { Guid.NewGuid() })
                .Create();

            RevertPaymentCommand sentCommand = null;

            _mockCommandDispatcher
                .Setup(m => m.SendMany(It.IsAny<IEnumerable<ICommand>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<ICommand>, CancellationToken>((c, t) =>
                {
                    sentCommand = c.Single() as RevertPaymentCommand;
                });

            // Act
            await _sut.RevertPayments(request);

            // Assert
            sentCommand.PaymentId.Should().Be(request.Payments[0]);
            sentCommand.ServiceRequestId.Should().Be(request.ServiceRequest.TaskId);
            sentCommand.DateServiceRequestTaskCreated.Should().Be(request.ServiceRequest.TaskCreatedDate);
            sentCommand.DecisionReferenceNumber.Should().Be(request.ServiceRequest.DecisionReference);
        }

        [Test]
        public async Task Then_multiple_revert_payment_commands_are_dispatched()
        {
            // Arrange
            var request = _fixture.Build<RevertPaymentsRequest>()
                .With(x => x.Payments, _fixture.CreateMany<Guid>(10).ToList())
                .Create();

            // Act
            await _sut.RevertPayments(request);

            // Assert
            _mockCommandDispatcher.Verify(
                x => x.SendMany(It.Is<IEnumerable<ICommand>>(y => y.Count() == 10), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Then_an_Ok_result_is_returned()
        {
            // Arrange
            var request = _fixture.Build<RevertPaymentsRequest>()
                .With(x => x.Payments, new List<Guid> { Guid.NewGuid() })
                .Create();

            // Act
            var result = await _sut.RevertPayments(request) as OkObjectResult;

            // Arrange
            result.Should().NotBeNull();
        }

        [Test]
        public async Task Then_a_BadRequest_result_is_returned()
        {
            // Arrange
            var request = _fixture.Build<RevertPaymentsRequest>()
                .With(x => x.Payments, new List<Guid> { Guid.NewGuid() })
                .Create();

            const string errorMessage = "Payment id not found";
            _mockCommandDispatcher
                .Setup(m => m.SendMany(It.IsAny<IEnumerable<ICommand>>(), It.IsAny<CancellationToken>()))
                .Throws(new ArgumentException(errorMessage));

            // Act
            var result =  await _sut.RevertPayments(request) as BadRequestObjectResult;
            result.Should().NotBeNull();
            result.Value.Should().Be(errorMessage);
        }
    }
}
