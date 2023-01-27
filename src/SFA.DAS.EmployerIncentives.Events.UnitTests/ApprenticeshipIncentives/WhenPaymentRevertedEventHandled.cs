using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Data.IncentiveApplication;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Events.UnitTests.ApprenticeshipIncentives
{
    [TestFixture]
    public class WhenPaymentRevertedEventHandled
    {
        private PaymentRevertedAuditHandler _sut;
        private Mock<IRevertedPaymentAuditRepository> _mockAuditDataRepository;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockAuditDataRepository = new Mock<IRevertedPaymentAuditRepository>();

            _sut = new PaymentRevertedAuditHandler(_mockAuditDataRepository.Object);
        }

        [Test]
        public async Task Then_an_audit_is_persisted()
        {
            //Arrange
            var model = _fixture.Create<PaymentModel>();

            var @event = new PaymentReverted(model, _fixture.Create<ServiceRequest>());

            //Act
            await _sut.Handle(@event);

            //Assert
            _mockAuditDataRepository.Verify(x =>
                    x.Add(It.Is<RevertedPaymentAudit>(
                            y => y.CalculatedDate == @event.Model.CalculatedDate &&
                            y.PaymentId == @event.Model.Id &&
                            y.ApprenticeshipIncentiveId == @event.Model.ApprenticeshipIncentiveId &&
                            y.PendingPaymentId == @event.Model.PendingPaymentId &&
                            y.PaidDate == @event.Model.PaidDate &&
                            y.Amount == @event.Model.Amount &&
                            y.PaymentPeriod == @event.Model.PaymentPeriod &&
                            y.PaymentYear == @event.Model.PaymentYear &&
                            y.VrfVendorId == @event.Model.VrfVendorId &&
                            y.ServiceRequest == @event.ServiceRequest)),
                Times.Once);
        }
    }
}
