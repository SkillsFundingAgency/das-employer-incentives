using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Events.UnitTests.ApprenticeshipIncentives
{
    [TestFixture]
    public class WhenPendingPaymentReinstated
    {
        private PendingPaymentReinstatedAuditHandler _sut;
        private Mock<IReinstatedPendingPaymentAuditRepository> _mockAuditRepository;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _mockAuditRepository = new Mock<IReinstatedPendingPaymentAuditRepository>();
            _sut = new PendingPaymentReinstatedAuditHandler(_mockAuditRepository.Object);
        }

        [Test]
        public async Task Then_an_audit_is_persisted_for_a_pending_payment_reinstated_event()
        {
            // Arrange
            var @event = _fixture.Create<PendingPaymentReinstated>();

            // Act
            await _sut.Handle(@event);

            // Assert
            _mockAuditRepository.Verify(x => x.Add(It.Is<ReinstatedPendingPaymentAudit>(i =>
                                i.ApprenticeshipIncentiveId == @event.Model.ApprenticeshipIncentiveId &&
                                i.PendingPaymentId == @event.Model.Id &&
                                i.ReinstatePaymentRequest == @event.ReinstatePaymentRequest)),
                                Times.Once);
        }
    }
}
