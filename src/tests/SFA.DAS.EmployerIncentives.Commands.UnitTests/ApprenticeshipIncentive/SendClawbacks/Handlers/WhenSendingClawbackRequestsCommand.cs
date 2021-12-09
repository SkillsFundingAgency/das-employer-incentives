using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SendClawbacks;
using SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.SendClawbackRequests.Handlers
{
    public class WhenSendingClawbackRequestsCommand
    {
        private SendClawbacksCommandHandler _sut;
        private Mock<IPaymentsQueryRepository> _mockPayableLegalEntityQueryRepository;
        private Mock<IPaymentDataRepository> _mockPaymentDataRepository;
        private Mock<IBusinessCentralFinancePaymentsService> _mockBusinessCentralFinancePaymentsService;
        private List<PaymentDto> _clawbacksToSend;
        private List<PaymentDto> _unsentClawbacks;
        private int _paymentRequestsLimit;

        private SendClawbacksCommand _command;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _paymentRequestsLimit = 3;

            _mockPayableLegalEntityQueryRepository = new Mock<IPaymentsQueryRepository>();
            _mockPaymentDataRepository = new Mock<IPaymentDataRepository>();
            _mockBusinessCentralFinancePaymentsService = new Mock<IBusinessCentralFinancePaymentsService>();
            _mockBusinessCentralFinancePaymentsService.Setup(x => x.PaymentRequestsLimit)
                .Returns(_paymentRequestsLimit);
            _clawbacksToSend = _fixture.CreateMany<PaymentDto>(5).ToList();
            _unsentClawbacks = _clawbacksToSend.TakeLast(5 - _paymentRequestsLimit).ToList();

            _command = new SendClawbacksCommand(_fixture.Create<long>(), _fixture.Create<DateTime>(), new Domain.ValueObjects.CollectionPeriod(_fixture.Create<byte>(), _fixture.Create<short>()));

            _sut = new SendClawbacksCommandHandler(_mockPaymentDataRepository.Object, _mockPayableLegalEntityQueryRepository.Object, _mockBusinessCentralFinancePaymentsService.Object);
        }

        [Test]
        public async Task Then_the_clawbacks_are_sent_to_business_central()
        {
            // Arrange
            SetupSingleCallScenario();

            // Act
            await _sut.Handle(_command);

            // Assert
            _mockBusinessCentralFinancePaymentsService.Verify(x =>
                x.SendPaymentRequests(It.Is<List<PaymentDto>>(p => p.Count == _paymentRequestsLimit && p[0] == _clawbacksToSend[0])));
        }

        [Test]
        public async Task Then_the_clawbacks_sent_to_business_central_have_the_clawback_date_updated()
        {
            // Arrange
            SetupSingleCallScenario();

            // Act
            await _sut.Handle(_command);

            // Assert
            _mockPaymentDataRepository.Verify(x =>
                x.RecordClawbacksSent(It.Is<List<Guid>>(l => l.Count == _paymentRequestsLimit),
                    _command.AccountLegalEntityId, _command.ClawbackDate));
        }

        [Test]
        public async Task Then_the_payments_are_sent_in_two_batches_to_business_central()
        {
            // Arrange
            SetupMultipleCallScenario();

            // Act
            await _sut.Handle(_command);

            // Assert
            _mockBusinessCentralFinancePaymentsService.Verify(x =>
                x.SendPaymentRequests(It.Is<List<PaymentDto>>(p => p.Count == _paymentRequestsLimit && p[0] == _clawbacksToSend[0])));
            _mockBusinessCentralFinancePaymentsService.Verify(x =>
                x.SendPaymentRequests(It.Is<List<PaymentDto>>(p => p.Count == 2 && p[0] == _clawbacksToSend[3])));
        }

        [Test]
        public async Task Then_the_clawbacks_sent_to_business_central_have_the_clawback_date_updated_first_and_second_call()
        {
            // Arrange
            SetupMultipleCallScenario();

            // Act
            await _sut.Handle(_command);

            // Assert
            _mockPaymentDataRepository.Verify(x =>
                x.RecordClawbacksSent(It.Is<List<Guid>>(l => l.Count == _paymentRequestsLimit),
                    _command.AccountLegalEntityId, _command.ClawbackDate));

            _mockPaymentDataRepository.Verify(x =>
                x.RecordClawbacksSent(It.Is<List<Guid>>(l => l.Count == _unsentClawbacks.Count),
                    _command.AccountLegalEntityId, _command.ClawbackDate));
        }


        public void SetupSingleCallScenario()
        {
            _mockPayableLegalEntityQueryRepository
                .Setup(x => x.GetUnpaidClawbacks(It.Is<long>(id => id == _command.AccountLegalEntityId)))
                .ReturnsAsync(_clawbacksToSend.Take(_paymentRequestsLimit).ToList());
        }
        public void SetupMultipleCallScenario()
        {
            _mockPayableLegalEntityQueryRepository
                .SetupSequence(x => x.GetUnpaidClawbacks(It.Is<long>(id => id == _command.AccountLegalEntityId)))
                .ReturnsAsync(_clawbacksToSend)
                .ReturnsAsync(_unsentClawbacks);
        }
    }
}
