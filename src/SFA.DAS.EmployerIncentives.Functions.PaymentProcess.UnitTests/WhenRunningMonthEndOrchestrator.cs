using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
{
    public class WhenRunningMonthEndOrchestrator
    {
        private Fixture _fixture;
        private CollectionPeriod _collectionPeriod;
        private Mock<IDurableOrchestrationContext> _mockOrchestrationContext;
        private MonthEndOrchestrator _sut;

        [SetUp]
        public void Setup()
        {
            // Arrange
            _fixture = new Fixture();
            _collectionPeriod = _fixture.Create<CollectionPeriod>();
            _mockOrchestrationContext = new Mock<IDurableOrchestrationContext>();
            _mockOrchestrationContext.Setup(x => x.GetInput<CollectionPeriod>()).Returns(_collectionPeriod);

            _sut = new MonthEndOrchestrator(Mock.Of<ILogger<MonthEndOrchestrator>>());
        }

        [Test]
        public async Task Then_LearnerMatchingOrchestrator_sub_orchestrator_is_called()
        {
            // Act
            await _sut.RunOrchestrator(_mockOrchestrationContext.Object);

            // Assert
            _mockOrchestrationContext.Verify(x => x.CallSubOrchestratorAsync(nameof(LearnerMatchingOrchestrator), null), Times.Once);
        }

        [Test]
        public async Task Then_IncentivePaymentOrchestrator_sub_orchestrator_is_called()
        {
            // Act
            await _sut.RunOrchestrator(_mockOrchestrationContext.Object);

            // Assert
            _mockOrchestrationContext.Verify(x => x.CallSubOrchestratorAsync(nameof(IncentivePaymentOrchestrator), _collectionPeriod), Times.Once);
        }

    }
}
