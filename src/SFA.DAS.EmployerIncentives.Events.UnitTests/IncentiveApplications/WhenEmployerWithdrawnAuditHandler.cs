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
    public class WhenEmployerWithdrawnAuditHandler
    {
        private EmployerWithdrawnAuditHandler _sut;
        private Mock<IIncentiveApplicationStatusAuditDataRepository> _mockAuditDataRepository;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockAuditDataRepository = new Mock<IIncentiveApplicationStatusAuditDataRepository>();

            _sut = new EmployerWithdrawnAuditHandler(_mockAuditDataRepository.Object);
        }

        [Test]
        public async Task Then_an_audit_is_persisted()
        {
            //Arrange
            var @event = _fixture.Create<EmployerWithdrawn>();

            //Act
            await _sut.Handle(@event);

            //Assert
            _mockAuditDataRepository.Verify(m =>
            m.Add(It.Is<IncentiveApplicationAudit>(i =>
                   i.IncentiveApplicationApprenticeshipId == @event.Model.Id &&
                   i.Process == Enums.IncentiveApplicationStatus.EmployerWithdrawn &&
                   i.ServiceRequest == @event.ServiceRequest)),
                   Times.Once);
        }
    }
}
