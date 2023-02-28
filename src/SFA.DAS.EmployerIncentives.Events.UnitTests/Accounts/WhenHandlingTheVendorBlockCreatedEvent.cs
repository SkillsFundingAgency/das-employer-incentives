using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Account;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Events.VendorBlocks;

namespace SFA.DAS.EmployerIncentives.Events.UnitTests.Accounts
{
    [TestFixture]
    public class WhenHandlingTheVendorBlockCreatedEvent
    {
        private VendorBlockCreatedAuditHandler _sut;
        private Mock<IVendorBlockAuditRepository> _repository;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _repository = new Mock<IVendorBlockAuditRepository>();
            _sut = new VendorBlockCreatedAuditHandler(_repository.Object);
        }

        [Test]
        public async Task Then_an_audit_is_persisted_for_a_vendor_block_created_event()
        {
            // Arrange
            var @event = _fixture.Create<VendorBlockCreated>();

            // Act
            await _sut.Handle(@event);

            // Assert
            _repository.Verify(x => x.Add(It.Is<VendorBlockRequestAudit>(
                y => y.VrfVendorId == @event.VendorId
                && y.VendorBlockEndDate == @event.VendorBlockEndDate
                && y.ServiceRequest.TaskId == @event.ServiceRequest.TaskId
                && y.ServiceRequest.DecisionReference == @event.ServiceRequest.DecisionReference
                && y.ServiceRequest.Created == @event.ServiceRequest.Created)), Times.Once);
        }

        [Test]
        public async Task Then_an_audit_is_not_persisted_for_a_vendor_block_created_event_without_a_ServiceRequest()
        {
            //Arrange
            var @event = _fixture
                .Build<VendorBlockCreated>()
                .Without(e => e.ServiceRequest)
                .Create();

            //Act
            await _sut.Handle(@event);

            //Assert
            _repository.Verify(m =>
                    m.Add(It.Is<VendorBlockRequestAudit>(i =>
                        i.VrfVendorId == @event.VendorId &&
                        i.ServiceRequest == @event.ServiceRequest)),
                Times.Never);
        }
    }

}
