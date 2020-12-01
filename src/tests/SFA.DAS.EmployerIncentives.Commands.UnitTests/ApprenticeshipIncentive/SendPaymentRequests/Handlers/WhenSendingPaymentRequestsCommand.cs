using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SendPaymentRequests;
using SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.SendPaymentRequests.Handlers
{
    public class WhenSendingPaymentRequestsCommand
    {
        private SendPaymentRequestsCommandHandler _sut;
        private Mock<IPayableLegalEntityQueryRepository> _mockPayableLegalEntityQueryRepository;
        private Mock<IAccountDataRepository> _mockAccountDataRepository;
        private Mock<IBusinessCentralFinancePaymentsService> _mockBusinessCentralFinancePaymentsService;
        private List<PaymentDto> _paymentsToSend;
        private List<PaymentDto> _paymentsSuccessfullySent;
        private List<PaymentDto> _unsentPayments;

        private SendPaymentRequestsCommand _command;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockPayableLegalEntityQueryRepository = new Mock<IPayableLegalEntityQueryRepository>();
            _mockAccountDataRepository = new Mock<IAccountDataRepository>();
            _mockBusinessCentralFinancePaymentsService = new Mock<IBusinessCentralFinancePaymentsService>();
            _paymentsToSend = _fixture.CreateMany<PaymentDto>(5).ToList();
            _paymentsSuccessfullySent = _paymentsToSend.Take(3).ToList();
            _unsentPayments = _paymentsToSend.TakeLast(2).ToList();

            _command = new SendPaymentRequestsCommand(_fixture.Create<long>(), _fixture.Create<DateTime>());

            _sut = new SendPaymentRequestsCommandHandler(_mockAccountDataRepository.Object, _mockPayableLegalEntityQueryRepository.Object, _mockBusinessCentralFinancePaymentsService.Object);
        }

        [Test]
        public async Task Then_the_payments_are_sent_to_business_central()
        {
            // Arrange
            SetupSingleCallScenario();

            // Act
            await _sut.Handle(_command);

            // Assert
            _mockBusinessCentralFinancePaymentsService.Verify(x=>x.SendPaymentRequestsForLegalEntity(_paymentsToSend));
        }

        [Test]
        public async Task Then_the_payments_sent_to_business_central_have_the_paid_date_updated()
        {
            // Arrange
            SetupSingleCallScenario();

            // Act
            await _sut.Handle(_command);

            // Assert
            _mockAccountDataRepository.Verify(x =>
                x.UpdatePaidDateForPaymentIds(It.Is<List<Guid>>(l => l.Count == _paymentsToSend.Count),
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
            _mockBusinessCentralFinancePaymentsService.Verify(x => x.SendPaymentRequestsForLegalEntity(_paymentsToSend));
            _mockBusinessCentralFinancePaymentsService.Verify(x => x.SendPaymentRequestsForLegalEntity(_unsentPayments));
        }

        [Test]
        public async Task Then_the_payments_sent_to_business_central_have_the_paid_date_updated_first_and_second_call()
        {
            // Arrange
            SetupMultipleCallScenario();

            // Act
            await _sut.Handle(_command);

            // Assert
            _mockAccountDataRepository.Verify(x =>
                x.UpdatePaidDateForPaymentIds(It.Is<List<Guid>>(l => l.Count == _paymentsSuccessfullySent.Count),
                    _command.AccountLegalEntityId, _command.PaidDate));

            _mockAccountDataRepository.Verify(x =>
                x.UpdatePaidDateForPaymentIds(It.Is<List<Guid>>(l => l.Count == _unsentPayments.Count),
                    _command.AccountLegalEntityId, _command.PaidDate));
        }


        public void SetupSingleCallScenario()
        {
            _mockPayableLegalEntityQueryRepository
                .Setup(x => x.GetPaymentsToSendForAccountLegalEntity(It.Is<long>(id => id == _command.AccountLegalEntityId))).ReturnsAsync(_paymentsToSend);

            _mockBusinessCentralFinancePaymentsService.Setup(x => x.SendPaymentRequestsForLegalEntity(_paymentsToSend))
                .ReturnsAsync(new SendPaymentsResponse(_paymentsToSend, true));
        }
        public void SetupMultipleCallScenario()
        {
            _mockPayableLegalEntityQueryRepository
                .SetupSequence(x => x.GetPaymentsToSendForAccountLegalEntity(It.Is<long>(id => id == _command.AccountLegalEntityId)))
                .ReturnsAsync(_paymentsToSend)
                .ReturnsAsync(_unsentPayments);

            _mockBusinessCentralFinancePaymentsService.SetupSequence(x => x.SendPaymentRequestsForLegalEntity(It.IsAny<List<PaymentDto>>()))
                .ReturnsAsync(new SendPaymentsResponse(_paymentsSuccessfullySent, false))
                .ReturnsAsync(new SendPaymentsResponse(_unsentPayments, true));
        }
    }
}
