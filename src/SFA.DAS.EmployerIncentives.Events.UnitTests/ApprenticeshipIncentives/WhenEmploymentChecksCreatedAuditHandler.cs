using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.UnitTests.ApprenticeshipIncentives
{
    public class WhenEmploymentChecksCreatedAuditHandler
    {
        private EmploymentChecksCreatedAuditHandler _sut;
        private Mock<IEmploymentCheckAuditRepository> _mockAuditRepository;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _mockAuditRepository = new Mock<IEmploymentCheckAuditRepository>();
            _sut = new EmploymentChecksCreatedAuditHandler(_mockAuditRepository.Object);
        }

        [Test]
        public async Task Then_an_audit_is_persisted_for_an_employment_checks_created_event()
        {
            //Arrange
            var @event = _fixture.Create<EmploymentChecksCreated>();

            //Act
            await _sut.Handle(@event);

            //Assert
            _mockAuditRepository.Verify(m =>
            m.Add(It.Is<EmploymentCheckRequestAudit>(i =>
                   i.ApprenticeshipIncentiveId == @event.ApprenticeshipIncentiveId &&
                   i.ServiceRequest == @event.ServiceRequest)),
                   Times.Once);
        }

        [Test]
        public async Task Then_an_audit_is_not_persisted_for_an_employment_checks_created_event_without_a_ServiceRequest()
        {
            //Arrange
            var @event = _fixture
                .Build<EmploymentChecksCreated>()
                .Without(e => e.ServiceRequest)
                .Create();

            //Act
            await _sut.Handle(@event);

            //Assert
            _mockAuditRepository.Verify(m =>
            m.Add(It.Is<EmploymentCheckRequestAudit>(i =>
                   i.ApprenticeshipIncentiveId == @event.ApprenticeshipIncentiveId &&
                   i.ServiceRequest == @event.ServiceRequest)),
                   Times.Never);
        }
    }
}
