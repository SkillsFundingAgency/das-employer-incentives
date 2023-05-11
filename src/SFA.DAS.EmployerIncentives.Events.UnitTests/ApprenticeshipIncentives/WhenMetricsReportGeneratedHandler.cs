using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.PaymentProcess;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Events.PaymentProcess;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.UnitTests.ApprenticeshipIncentives
{
    public class WhenMetricsReportGeneratedHandler
    {
        private MetricsReportGeneratedHandler _sut;
        private Mock<ICommandPublisher> _mockCommandPublisher;
        
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockCommandPublisher = new Mock<ICommandPublisher>();
            
            _sut = new MetricsReportGeneratedHandler(_mockCommandPublisher.Object);
        }

        [Test]
        public async Task Then_a_SlackNotificationCommand_is_published()
        {
            //Arrange
            var @event = _fixture.Create<MetricsReportGenerated>();

            //Act
            await _sut.Handle(@event);

            //Assert
            _mockCommandPublisher.Verify(m => m.Publish(It.Is<SlackNotificationCommand>(i => 
                i.SlackMessage.Content.Contains(nameof(MetricsReportGenerated))),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
