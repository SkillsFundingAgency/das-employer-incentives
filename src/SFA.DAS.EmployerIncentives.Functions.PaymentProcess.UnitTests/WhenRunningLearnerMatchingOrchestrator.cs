using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            _mockOrchestrationContext
                .Setup(x => x.CallActivityAsync<List<ApprenticeshipIncentiveOutput>>("GetAllApprenticeshipIncentives",
                    null)).ReturnsAsync(_apprenticeshipIncentives);

            _orchestrator = new LearnerMatchingOrchestrator(Mock.Of<ILogger<LearnerMatchingOrchestrator>>());
        }

        [Test]
        public async Task Then_query_is_called_to_get_apprenticeship_Incentives()
        {
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(
                x => x.CallActivityAsync<List<ApprenticeshipIncentiveOutput>>("GetAllApprenticeshipIncentives", null),
                Times.Once);
        }

        [Test]
        public async Task Then_LearnerMatchingApprenticeshipOrchestrator_is_called_for_each_apprenticeship_incentive()
        {
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            foreach (var i in _apprenticeshipIncentives)
            {
                _mockOrchestrationContext.Verify(
                    x => x.CallSubOrchestratorAsync(
                        nameof(LearnerMatchingApprenticeshipOrchestrator),
                        It.Is<ApprenticeshipIncentiveOutput>(
                            input => input.Id == i.Id && input.ULN == i.ULN)), Times.Once);
            }
        }
    }
}