using AutoFixture;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
{
    public class WhenRunningChangeOfCircumstanceOrchestrator
    {
        private Fixture _fixture;
        private LearnerChangeOfCircumstanceInput _changeOfCircumstanceInput;
        private Mock<TaskOrchestrationContext> _mockOrchestrationContext;
        private ChangeOfCircumstanceOrchestrator _orchestrator;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _changeOfCircumstanceInput = _fixture.Create<LearnerChangeOfCircumstanceInput>();
            _mockOrchestrationContext = new Mock<TaskOrchestrationContext>();
            _mockOrchestrationContext.Setup(x => x.GetInput<LearnerChangeOfCircumstanceInput>()).Returns(_changeOfCircumstanceInput);

            _mockOrchestrationContext.Setup(x => x.CallActivityAsync<bool>("ApprenticeshipIncentiveHasPossibleChangeOrCircs", _changeOfCircumstanceInput.ApprenticeshipIncentiveId, It.IsAny<TaskOptions>())).ReturnsAsync(true);
            _orchestrator = new ChangeOfCircumstanceOrchestrator(Mock.Of<ILogger<ChangeOfCircumstanceOrchestrator>>());
        }

        [Test]
        public async Task Then_learner_change_of_circumstance_is_triggered()
        {
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(x => x.CallActivityAsync("LearnerChangeOfCircumstanceActivity", _changeOfCircumstanceInput, It.IsAny<TaskOptions>()), Times.Once);
        }        

        [Test]
        public async Task Then_learner_match_is_called_after_recalculating_earnings()
        {
            var sequence = new MockSequence();
            _mockOrchestrationContext.InSequence(sequence).Setup(
                x => x.CallActivityAsync("CalculateEarningsActivity",
                    It.Is<LearnerMatchInput>(y =>  y.ApprenticeshipIncentiveId == _changeOfCircumstanceInput.ApprenticeshipIncentiveId), It.IsAny<TaskOptions>()));

            _mockOrchestrationContext.InSequence(sequence).Setup(x => x.CallActivityAsync("LearnerMatchAndUpdate",
                It.Is<LearnerMatchInput>(y => y.ApprenticeshipIncentiveId == _changeOfCircumstanceInput.ApprenticeshipIncentiveId), It.IsAny<TaskOptions>()));

            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(
                x => x.CallActivityAsync("LearnerMatchAndUpdate",
                    It.Is<LearnerMatchInput>(y => y.ApprenticeshipIncentiveId == _changeOfCircumstanceInput.ApprenticeshipIncentiveId),
                    It.IsAny<TaskOptions>()), Times.Once);
        }

        [Test]
        public async Task Then_change_of_circumstances_is_not_triggered_when_there_is_no_potential_change()
        {
            _mockOrchestrationContext.Setup(x => x.CallActivityAsync<bool>("ApprenticeshipIncentiveHasPossibleChangeOrCircs", _changeOfCircumstanceInput.ApprenticeshipIncentiveId, It.IsAny<TaskOptions>())).ReturnsAsync(false);

            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(x => x.CallActivityAsync("LearnerChangeOfCircumstanceActivity", It.IsAny<LearnerChangeOfCircumstanceInput>(), It.IsAny<TaskOptions>()), Times.Never);
            _mockOrchestrationContext.Verify(x => x.CallActivityAsync("CalculateEarningsActivity", It.IsAny<CalculateEarningsInput>(), It.IsAny<TaskOptions>()), Times.Never);
            _mockOrchestrationContext.Verify(x => x.CallActivityAsync("LearnerMatchAndUpdate", It.IsAny<LearnerMatchInput>(), It.IsAny<TaskOptions>()), Times.Never);
        }
    }
}