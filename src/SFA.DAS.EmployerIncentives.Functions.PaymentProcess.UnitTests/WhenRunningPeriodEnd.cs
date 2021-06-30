using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
{
    public class WhenRunningPeriodEnd
    {
        private Mock<ILogger<PeriodEndOrchestrator>> _mockLogger;
        private PeriodEndOrchestrator _orchestrator;
        private Mock<IDurableOrchestrationContext> _mockOrchestrationContext;

        [SetUp]
        public void Setup()
        {
            _mockOrchestrationContext = new Mock<IDurableOrchestrationContext>();
            _mockLogger = new Mock<ILogger<PeriodEndOrchestrator>>();

            _orchestrator = new PeriodEndOrchestrator(_mockLogger.Object);
        }

        [Test]
        public async Task Then_sub_orchestrator_is_called_to_learner_match()
        {
            // act
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(x => x.CallSubOrchestratorAsync(
                "LearnerMatchingOrchestrator",
                null
            ), Times.Once);
        }

        [Test]
        public async Task Then_sub_orchestrator_is_called_to_create_and_send_payments()
        {
            // act
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(x => x.CallSubOrchestratorAsync(
                "IncentivePaymentOrchestrator",
                It.IsAny<string>(),
                null
            ), Times.Once);
        }
    }
}
