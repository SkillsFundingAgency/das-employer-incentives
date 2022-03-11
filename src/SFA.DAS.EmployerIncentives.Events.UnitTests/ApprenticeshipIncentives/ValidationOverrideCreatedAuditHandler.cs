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
    public class WhenValidationOverrideCreatedAuditHandler
    {
        private ValidationOverrideCreatedAuditHandler _sut;
        private Mock<IValidationOverrideAuditRepository> _mockAuditRepository;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _mockAuditRepository = new Mock<IValidationOverrideAuditRepository>();
            _sut = new ValidationOverrideCreatedAuditHandler(_mockAuditRepository.Object);
        }

        [Test]
        public async Task Then_an_audit_is_persisted_for_a_validation_override_created_event()
        {
            //Arrange
            var @event = _fixture.Create<ValidationOverrideCreated>();

            //Act
            await _sut.Handle(@event);

            //Assert
            _mockAuditRepository.Verify(m =>
            m.Add(It.Is<ValidationOverrideStepAudit>(i =>
                   i.Id == @event.ValidationOverrideId &&
                   i.ApprenticeshipIncentiveId == @event.ApprenticeshipIncentiveId &&
                   i.ValidationOverrideStep == @event.ValidationOverrideStep &&
                   i.ServiceRequest == @event.ServiceRequest)),
                   Times.Once);
        }
    }
}
