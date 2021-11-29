using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Events.UnitTests.ApprenticeshipIncentives
{
    public class WhenEmploymentChecksCreatedHandler
    {
        private EmploymentChecksCreatedHandler _sut;
        private Mock<ICommandPublisher> _mockCommandPublisher;
        
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockCommandPublisher = new Mock<ICommandPublisher>();
            
            _sut = new EmploymentChecksCreatedHandler(_mockCommandPublisher.Object);
        }

        [Test]
        public async Task Then_a_SendEmploymentCheckRequestsCommand_is_published()
        {
            //Arrange
            var @event = _fixture.Create<EmploymentChecksCreated>();

            //Act
            await _sut.Handle(@event);

            //Assert
            _mockCommandPublisher.Verify(m => m.Publish(It.Is<SendEmploymentCheckRequestsCommand>(i => 
                i.ApprenticeshipIncentiveId == @event.ApprenticeshipIncentiveId), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
