using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Events;
using SFA.DAS.EmployerIncentives.Events.Accounts;

namespace SFA.DAS.EmployerIncentives.Events.UnitTests.Accounts
{
    public class WhenHandlingTheBankDetailsApprovedForLegalEntityEvent
    {
        private BankDetailsApprovedForLegalEntityHandler _sut;
        private Mock<ICommandPublisher> _mockCommandPublisher;
        
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockCommandPublisher = new Mock<ICommandPublisher>();
            
            _sut = new BankDetailsApprovedForLegalEntityHandler(_mockCommandPublisher.Object);
        }

        [Test]
        public async Task Then_a_CompletePaymentsCalculationCommand_is_published()
        {
            //Arrange
            var @event = _fixture.Create<BankDetailsApprovedForLegalEntity>();

            //Act
            await _sut.Handle(@event);

            //Assert
            _mockCommandPublisher.Verify(m => m.Publish(It.Is<AddEmployerVendorIdCommand>(i => 
                i.HashedLegalEntityId == @event.HashedLegalEntityId), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
