using System;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.PausePayments;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
{
    public class WhenPausingOrResumingPayments
    {
        private Fixture _fixture;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;
        private Mock<ILogger> _mockLogger;
        private HandlePausePaymentsRequest _sut;
        private long _accountLegalEntityId;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();

            _accountLegalEntityId = _fixture.Create<long>();
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            _mockLogger = new Mock<ILogger>();

            _sut = new HandlePausePaymentsRequest(_mockCommandDispatcher.Object);
        }

        [Test]
        public async Task Then_PausePaymentsCommand_is_created_and_dispatched()
        {
            var request = _fixture.Create<PausePaymentsRequest>();
            
            await SendRequestToEndpoint(request);

            _mockCommandDispatcher.Verify(x =>
                x.Send(It.Is<PausePaymentsCommand>(c =>
                        c.AccountLegalEntityId == _accountLegalEntityId && c.Action == request.Action &&
                        c.DateServiceRequestTaskCreated == request.DateServiceRequestTaskCreated &&
                        c.DecisionReferenceNumber == request.DecisionReferenceNumber &&
                        c.ServiceRequestId == request.ServiceRequestId && c.ULN == request.ULN),
                    It.IsAny<CancellationToken>()));
        }

        [TestCase(PausePaymentsAction.Pause)]
        [TestCase(PausePaymentsAction.Resume)]
        public async Task Then_returns_expected_successful_response(PausePaymentsAction action)
        {
            var request = _fixture.Build<PausePaymentsRequest>().With(x=>x.Action, action).Create();

            var response = (await SendRequestToEndpoint(request)) as OkObjectResult;

            response.Should().NotBeNull();
            JsonConvert.SerializeObject(response.Value).Should().Contain($"Payments have been successfully {action}d");
        }

        [Test]
        public async Task Then_returns_bad_request_response_when_InvalidRequestException_is_thrown()
        {
            _mockCommandDispatcher.Setup(x => x.Send(It.IsAny<ICommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidRequestException(new Dictionary<string, string> {{"ULN", "Is not set"}}));           
            var request = _fixture.Create<PausePaymentsRequest>();

            var response = (await SendRequestToEndpoint(request)) as BadRequestObjectResult;

            response.Should().NotBeNull();
            JsonConvert.SerializeObject(response.Value).Should().Contain("Is not set");
        }

        [Test]
        public async Task Then_returns_not_found_response_when_no_matching_record_is_found()
        {
            _mockCommandDispatcher.Setup(x => x.Send(It.IsAny<ICommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException("Not Found"));
            var request = _fixture.Create<PausePaymentsRequest>();

            var response = (await SendRequestToEndpoint(request)) as NotFoundObjectResult;

            response.Should().NotBeNull();
            JsonConvert.SerializeObject(response.Value).Should().Contain("Not Found");
        }

        [Test]
        public async Task Then_returns_bad_request_response_when_PausePaymentException_is_thrown()
        {
            _mockCommandDispatcher.Setup(x => x.Send(It.IsAny<ICommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new PausePaymentsException("Problem"));
            var request = _fixture.Create<PausePaymentsRequest>();

            var response = (await SendRequestToEndpoint(request)) as BadRequestObjectResult;

            response.Should().NotBeNull();
            JsonConvert.SerializeObject(response.Value).Should().Contain("Problem");
        }

        [Test]
        public async Task Then_returns_internal_server_error_response_when_any_other_exception_is_thrown()
        {
            _mockCommandDispatcher.Setup(x => x.Send(It.IsAny<ICommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Internal error"));
            var request = _fixture.Create<PausePaymentsRequest>();

            var response = (await SendRequestToEndpoint(request)) as InternalServerErrorResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be((int) HttpStatusCode.InternalServerError);
        }



        private Task<IActionResult> SendRequestToEndpoint(PausePaymentsRequest request)
        {
            var httpRequest = new HttpRequestMessage();
            var body = JsonConvert.SerializeObject(request);
            httpRequest.Content = new StringContent(body);

            return _sut.Run(httpRequest, _accountLegalEntityId, _mockLogger.Object);
        }

    }
}