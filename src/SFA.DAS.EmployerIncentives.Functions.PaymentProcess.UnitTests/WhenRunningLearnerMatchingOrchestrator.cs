using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
{
    public class WhenRunningLearnerMatchingOrchestrator
    {
        private Fixture _fixture;
        private Mock<IDurableOrchestrationContext> _mockOrchestrationContext;
        private LearnerMatchingOrchestrator _orchestrator;
        private List<ApprenticeshipIncentiveOutput> _apprenticeshipIncentives;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _mockOrchestrationContext = new Mock<IDurableOrchestrationContext>();

            _apprenticeshipIncentives = _fixture.CreateMany<ApprenticeshipIncentiveOutput>(3).ToList();
            _mockOrchestrationContext.Setup(x => x.CallActivityAsync<List<ApprenticeshipIncentiveOutput>>("GetAllApprenticeshipIncentives", null)).ReturnsAsync(_apprenticeshipIncentives);

            _orchestrator = new LearnerMatchingOrchestrator(Mock.Of<ILogger<LearnerMatchingOrchestrator>>());
        }

        [Test]
        public async Task Then_learner_match_is_not_performed_if_payment_run_is_in_progress()
        {
            _mockOrchestrationContext.Setup(x => x.CallActivityAsync<bool>("CollectionPeriodInProgress", null)).ReturnsAsync(true);

            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(x => x.CallActivityAsync<List<ApprenticeshipIncentiveOutput>>("GetAllApprenticeshipIncentives", null), Times.Never);
        }

        [Test]
        public async Task Then_query_is_called_to_get_apprenticeship_Incentives()
        {
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(x => x.CallActivityAsync<List<ApprenticeshipIncentiveOutput>>("GetAllApprenticeshipIncentives", null), Times.Once);
        }

        [Test]
        public async Task Then_activity_to_match_learner_is_called_for_each_apprenticeship_incentive()
        {
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(x => x.CallActivityWithRetryAsync(
                "LearnerMatchAndUpdate",
                It.IsAny<RetryOptions>(),
                It.IsAny<LearnerMatchInput>()
            ), Times.Exactly(_apprenticeshipIncentives.Count));

            foreach (var apprenticeshipIncentive in _apprenticeshipIncentives)
            {
                _mockOrchestrationContext.Verify(x => x.CallActivityWithRetryAsync("LearnerMatchAndUpdate", It.IsAny<RetryOptions>(), It.Is<LearnerMatchInput>(input => input.ApprenticeshipIncentiveId == apprenticeshipIncentive.Id)), Times.Once);
            }
        }

        [Test]
        public async Task Then_activity_to_change_circumstances_is_called_for_each_apprenticeship_incentive()
        {
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            foreach (var apprenticeshipIncentive in _apprenticeshipIncentives)
            {
                _mockOrchestrationContext.Verify(x => x.CallSubOrchestratorAsync("ChangeOfCircumstanceOrchestrator", It.Is<LearnerChangeOfCircumstanceInput>(input => input.ApprenticeshipIncentiveId == apprenticeshipIncentive.Id && input.Uln == apprenticeshipIncentive.ULN)), Times.Once);
            }
        }
    }
}