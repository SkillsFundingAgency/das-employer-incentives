using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.IncentiveApplication;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Events.IncentiveApplications;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.UnitTests.IncentiveApplications
{
    public class WhenApplicationWithdrawnAuditHandler
    {
        private Mock<IIncentiveApplicationStatusAuditDataRepository> _mockAuditDataRepository;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockAuditDataRepository = new Mock<IIncentiveApplicationStatusAuditDataRepository>();
        }

        [Test]
        public async Task Then_an_audit_is_persisted_for_an_employer_withdrawal()
        {
            //Arrange
            var sut = new EmployerWithdrawnAuditHandler(_mockAuditDataRepository.Object);
            var @event = _fixture.Create<EmployerWithdrawn>();

            //Act
            await sut.Handle(@event);

            //Assert
            _mockAuditDataRepository.Verify(m =>
            m.Add(It.Is<IncentiveApplicationAudit>(i =>
                   i.IncentiveApplicationApprenticeshipId == @event.Model.Id &&
                   i.Process == Enums.IncentiveApplicationStatus.EmployerWithdrawn &&
                   i.ServiceRequest == @event.ServiceRequest)),
                   Times.Once);
        }

        [Test]
        public async Task Then_an_audit_is_persisted_for_a_compliance_withdrawal()
        {
            //Arrange
            var sut = new ComplianceWithdrawnAuditHandler(_mockAuditDataRepository.Object);
            var @event = _fixture.Create<ComplianceWithdrawn>();

            //Act
            await sut.Handle(@event);

            //Assert
            _mockAuditDataRepository.Verify(m =>
            m.Add(It.Is<IncentiveApplicationAudit>(i =>
                   i.IncentiveApplicationApprenticeshipId == @event.Model.Id &&
                   i.Process == Enums.IncentiveApplicationStatus.ComplianceWithdrawn &&
                   i.ServiceRequest == @event.ServiceRequest)),
                   Times.Once);
        }
    }
}
