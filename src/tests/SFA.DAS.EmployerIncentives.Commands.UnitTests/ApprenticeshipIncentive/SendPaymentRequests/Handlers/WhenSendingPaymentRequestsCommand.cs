﻿using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SendPaymentRequests;
using SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.SendPaymentRequests.Handlers
{
    public class WhenSendingPaymentRequestsCommand
    {
        private SendPaymentRequestsCommandHandler _sut;
        private Mock<IPaymentsQueryRepository> _mockPayableLegalEntityQueryRepository;
        private Mock<IPaymentDataRepository> _mockPaymentDataRepository;
        private Mock<IBusinessCentralFinancePaymentsService> _mockBusinessCentralFinancePaymentsService;
        private List<Payment> _paymentsToSend;
        private List<Payment> _unsentPayments;
        private int _paymentRequestsLimit;

        private SendPaymentRequestsCommand _command;

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
            _paymentsToSend = _fixture.CreateMany<Payment>(5).ToList();
            _unsentPayments = _paymentsToSend.TakeLast(5 - _paymentRequestsLimit).ToList();

            _command = new SendPaymentRequestsCommand(_fixture.Create<long>(), _fixture.Create<DateTime>(), new Domain.ValueObjects.CollectionPeriod(_fixture.Create<byte>(), _fixture.Create<short>()));

            _sut = new SendPaymentRequestsCommandHandler(_mockPaymentDataRepository.Object, _mockPayableLegalEntityQueryRepository.Object, _mockBusinessCentralFinancePaymentsService.Object);
        }

        [Test]
        public async Task Then_the_payments_are_sent_to_business_central()
        {
            // Arrange
            SetupSingleCallScenario();

            // Act
            await _sut.Handle(_command);

            // Assert
            _mockBusinessCentralFinancePaymentsService.Verify(x =>
                x.SendPaymentRequests(It.Is<List<Payment>>(p => p.Count == _paymentRequestsLimit && p[0] == _paymentsToSend[0])));
        }

        [Test]
        public async Task Then_the_payments_sent_to_business_central_have_the_paid_date_updated()
        {
            // Arrange
            SetupSingleCallScenario();

            // Act
            await _sut.Handle(_command);

            // Assert
            _mockPaymentDataRepository.Verify(x =>
                x.RecordPaymentsSent(It.Is<List<Guid>>(l => l.Count == _paymentRequestsLimit),
                    _command.AccountLegalEntityId, _command.PaidDate));
        }

        [Test]
        public async Task Then_the_payments_are_sent_twice_to_business_central()
        {
            // Arrange
            SetupMultipleCallScenario();

            // Act
            await _sut.Handle(_command);

            // Assert
            _mockBusinessCentralFinancePaymentsService.Verify(x =>
                x.SendPaymentRequests(It.Is<List<Payment>>(p => p.Count == _paymentRequestsLimit && p[0] == _paymentsToSend[0])));
            _mockBusinessCentralFinancePaymentsService.Verify(x =>
                x.SendPaymentRequests(It.Is<List<Payment>>(p => p.Count == 2 && p[0] == _paymentsToSend[3])));
        }

        [Test]
        public async Task Then_the_payments_sent_to_business_central_have_the_paid_date_updated_first_and_second_call()
        {
            // Arrange
            SetupMultipleCallScenario();

            // Act
            await _sut.Handle(_command);

            // Assert
            _mockPaymentDataRepository.Verify(x =>
                x.RecordPaymentsSent(It.Is<List<Guid>>(l => l.Count == _paymentRequestsLimit),
                    _command.AccountLegalEntityId, _command.PaidDate));

            _mockPaymentDataRepository.Verify(x =>
                x.RecordPaymentsSent(It.Is<List<Guid>>(l => l.Count == _unsentPayments.Count),
                    _command.AccountLegalEntityId, _command.PaidDate));
        }

        public void SetupSingleCallScenario()
        {
            _mockPayableLegalEntityQueryRepository
                .Setup(x => x.GetUnpaidPayments(It.Is<long>(id => id == _command.AccountLegalEntityId)))
                .ReturnsAsync(_paymentsToSend.Take(_paymentRequestsLimit).ToList());
        }
        public void SetupMultipleCallScenario()
        {
            _mockPayableLegalEntityQueryRepository
                .SetupSequence(x => x.GetUnpaidPayments(It.Is<long>(id => id == _command.AccountLegalEntityId)))
                .ReturnsAsync(_paymentsToSend)
                .ReturnsAsync(_unsentPayments);
        }
    }
}
