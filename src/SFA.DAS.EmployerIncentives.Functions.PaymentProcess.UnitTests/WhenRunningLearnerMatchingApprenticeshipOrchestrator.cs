using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
{
    public class WhenRunningLearnerMatchingApprenticeshipOrchestrator
    {
        private Fixture _fixture;
        private Mock<IDurableOrchestrationContext> _mockOrchestrationContext;
        private LearnerMatchingApprenticeshipOrchestrator _orchestrator;
        private ApprenticeshipIncentiveOutput _expectedInput;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _mockOrchestrationContext = new Mock<IDurableOrchestrationContext>();
            _expectedInput = _fixture.Create<ApprenticeshipIncentiveOutput>();
            _mockOrchestrationContext.Setup(x => x.GetInput<ApprenticeshipIncentiveOutput>()).Returns(_expectedInput);
            _orchestrator = new LearnerMatchingApprenticeshipOrchestrator(Mock.Of<ILogger<LearnerMatchingApprenticeshipOrchestrator>>());
        }

        [Test]
        public async Task Then_PerformLearnerMatch_activity_is_called()
        {
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);
            
            _mockOrchestrationContext.Verify(
                x => x.CallActivityWithRetryAsync(nameof(LearnerMatchAndUpdate), It.IsAny<RetryOptions>(),
                    It.Is<LearnerMatchInput>(input => input.ApprenticeshipIncentiveId == _expectedInput.Id)),
                Times.Once);
        }

        [Test]
        public async Task Then_CalculateDaysInLearning_activity_is_called()
        {
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);
            
            _mockOrchestrationContext.Verify(
                x => x.CallActivityAsync(nameof(CalculateDaysInLearning),
                    It.Is<CalculateDaysInLearningInput>(input => input.ApprenticeshipIncentiveId == _expectedInput.Id)),
                Times.Once);
        }

        [Test]
        public async Task Then_ChangeOfCircumstanceOrchestrator_is_called()
        {
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(
                x => x.CallSubOrchestratorAsync(nameof(ChangeOfCircumstanceOrchestrator),
                    It.Is<LearnerChangeOfCircumstanceInput>(input =>
                        input.ApprenticeshipIncentiveId == _expectedInput.Id &&
                        input.Uln == _expectedInput.ULN)), Times.Once);
        }

        [Test]
        public async Task Then_SetSuccessfulLearnerMatch_activity_is_called()
        {
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(
                x => x.CallActivityAsync(nameof(SetSuccessfulLearnerMatch),
                    It.Is<SetSuccessfulLearnerMatchInput>(input =>
                        input.ApprenticeshipIncentiveId == _expectedInput.Id &&
                        input.Uln == _expectedInput.ULN &&
                        input.Succeeded == false
                        )), Times.Once);

            _mockOrchestrationContext.Verify(
                x => x.CallActivityAsync(nameof(SetSuccessfulLearnerMatch),
                    It.Is<SetSuccessfulLearnerMatchInput>(input =>
                        input.ApprenticeshipIncentiveId == _expectedInput.Id &&
                        input.Uln == _expectedInput.ULN &&
                        input.Succeeded == true
                        )), Times.Once);
        }
    }
}