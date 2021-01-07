using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.IncentiveApplication;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Events.UnitTests.ApprenticeshipIncentives
{
    public class WhenPaymentsPausedEventIsHandled
    {
        private PaymentsPausedHandler _sut;
        private Mock<IIncentiveApplicationStatusAuditDataRepository> _mockAuditDataRepository;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockAuditDataRepository = new Mock<IIncentiveApplicationStatusAuditDataRepository>();

            _sut = new PaymentsPausedHandler(_mockAuditDataRepository.Object);
        }

        [Test]
        public async Task Then_an_audit_is_persisted()
        {
            //Arrange
            var @event = _fixture.Create<PaymentsPaused>();

            //Act
            await _sut.Handle(@event);

            //Assert
            _mockAuditDataRepository.Verify(m =>
            m.Add(It.Is<IncentiveApplicationAudit>(i =>
                   i.IncentiveApplicationApprenticeshipId == @event.Model.ApplicationApprenticeshipId &&
                   i.Process == Enums.IncentiveApplicationStatus.PaymentsPaused &&
                   i.ServiceRequest == @event.ServiceRequest)),
                   Times.Once);
        }
    }
}
