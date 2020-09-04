using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.Application;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;
using SFA.DAS.EmployerIncentives.Events.IncentiveApplications;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Application.UnitTests.AddLegalEntity.Handlers
{
    public class WhenSubmittedEventHandler
    {
        private SubmittedEventHandler _sut;
        private Mock<ICommandPublisher> _mockCommandPublisher;
        
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockCommandPublisher = new Mock<ICommandPublisher>();
            
            _sut = new SubmittedEventHandler(_mockCommandPublisher.Object);
        }

        [Test]
        public async Task Then_a_CalculateEarningsCommand_is_published()
        {
            //Arrange
            var @event = _fixture.Create<Submitted>();

            //Act
            await _sut.Handle(@event);

            //Assert
            _mockCommandPublisher.Verify(m => m.Publish(It.Is<CalculateClaimCommand>(i => 
                i.AccountId == @event.AccountId &&
                i.IncentiveClaimApplicationId == @event.IncentiveClaimApplicationId), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
