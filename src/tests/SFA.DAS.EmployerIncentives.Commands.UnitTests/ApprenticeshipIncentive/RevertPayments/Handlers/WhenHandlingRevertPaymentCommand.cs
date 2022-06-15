using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.RevertPayments;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.RevertPayments.Handlers
{
    [TestFixture]
    public class WhenHandlingRevertPaymentCommand
    {
        private RevertPaymentCommandHandler _sut;
        private RevertPaymentCommand _command;
        private Guid _pendingPaymentId;
        private Guid _apprenticeshipIncentiveId;
        private Domain.ApprenticeshipIncentives.ApprenticeshipIncentive _incentive;
        private Mock<IApprenticeshipIncentiveDomainRepository> _mockDomainRepository;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _mockDomainRepository = new Mock<IApprenticeshipIncentiveDomainRepository>();
            _sut = new RevertPaymentCommandHandler(_mockDomainRepository.Object);
            _command = _fixture.Create<RevertPaymentCommand>();
            _pendingPaymentId = Guid.NewGuid();
            _apprenticeshipIncentiveId = Guid.NewGuid();
            _incentive = Domain.ApprenticeshipIncentives.ApprenticeshipIncentive.Get(_apprenticeshipIncentiveId, 
                _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(x => x.PendingPaymentModels, new List<PendingPaymentModel> 
                {
                    _fixture.Build<PendingPaymentModel>()
                        .With(x => x.Id, _pendingPaymentId)
                        .With(x => x.ApprenticeshipIncentiveId, _apprenticeshipIncentiveId)
                        .With(x => x.PaymentMadeDate, _fixture.Create<DateTime>())
                        .Create()
                })
                .With(x => x.PaymentModels, new List<PaymentModel> 
                { 
                    _fixture.Build<PaymentModel>()
                        .With(x => x.Id, _command.PaymentId)
                        .With(x => x.PendingPaymentId, _pendingPaymentId)
                        .With(x => x.ApprenticeshipIncentiveId, _apprenticeshipIncentiveId)
                        .With(x => x.PaidDate, _fixture.Create<DateTime>())
                        .Create()
                })
                .Create());
        }

        [Test]
        public async Task Then_the_payment_is_reverted_for_the_specified_apprenticeship_incentive()
        {
            // Arrange
            _mockDomainRepository.Setup(x => x.FindByPaymentId(_command.PaymentId)).ReturnsAsync(_incentive);

            // Act
            await _sut.Handle(_command);

            // Assert
            _mockDomainRepository.Verify(x => x.Save(It.Is<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>(
                y => y.Payments.Count == 0 
                && y.PendingPayments.Count == 1 
                && !y.PendingPayments.ToList()[0].PaymentMadeDate.HasValue)), Times.Once);
        }

        [Test]
        public void Then_an_exception_is_thrown_if_the_incentive_cannot_be_found_by_payment_id()
        {
            // Arrange
            const string errorMessage = "Incentive not found";
            _mockDomainRepository.Setup(x => x.FindByPaymentId(_command.PaymentId)).ThrowsAsync(new ArgumentException(errorMessage));

            // Act
            Func<Task> action = async () => await _sut.Handle(_command);

            // Assert
            action.Should().Throw<ArgumentException>().WithMessage(errorMessage);
        }
    }
}
