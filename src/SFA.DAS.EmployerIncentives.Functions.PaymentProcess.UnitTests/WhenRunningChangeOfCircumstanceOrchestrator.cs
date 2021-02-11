using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
{
    public class WhenRunningChangeOfCircumstanceOrchestrator
    {
        private Fixture _fixture;
        private LearnerChangeOfCircumstanceInput _changeOfCircumstanceInput;
        private Mock<IDurableOrchestrationContext> _mockOrchestrationContext;
        private ChangeOfCircumstanceOrchestrator _orchestrator;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _changeOfCircumstanceInput = _fixture.Create<LearnerChangeOfCircumstanceInput>();
            _mockOrchestrationContext = new Mock<IDurableOrchestrationContext>();
            _mockOrchestrationContext.Setup(x => x.GetInput<LearnerChangeOfCircumstanceInput>()).Returns(_changeOfCircumstanceInput);

            _mockOrchestrationContext.Setup(x => x.CallActivityAsync<bool>("ApprenticeshipIncentiveHasPossibleChangeOrCircs", _changeOfCircumstanceInput.ApprenticeshipIncentiveId)).ReturnsAsync(true);
            _orchestrator = new ChangeOfCircumstanceOrchestrator(Mock.Of<ILogger<ChangeOfCircumstanceOrchestrator>>());
        }

        [Test]
        public async Task Then_learner_change_of_circumstance_is_triggered()
        {
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(x => x.CallActivityAsync("LearnerChangeOfCircumstanceActivity", _changeOfCircumstanceInput), Times.Once);
        }

        [Test]
        public async Task Then_earnings_are_recalculated_after_the_change_of_circumstances()
        {
            var sequence = new MockSequence();
            _mockOrchestrationContext.InSequence(sequence).Setup(x => x.CallActivityAsync("LearnerChangeOfCircumstanceActivity", _changeOfCircumstanceInput));
            _mockOrchestrationContext.InSequence(sequence).Setup(
                x => x.CallActivityAsync("CalculateEarningsActivity",
                    It.Is<CalculateEarningsInput>(y =>  y.ApprenticeshipIncentiveId == _changeOfCircumstanceInput.ApprenticeshipIncentiveId &&  y.Uln == _changeOfCircumstanceInput.Uln)));

            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(
                x => x.CallActivityAsync("CalculateEarningsActivity",
                    It.Is<CalculateEarningsInput>(y =>
                        y.ApprenticeshipIncentiveId == _changeOfCircumstanceInput.ApprenticeshipIncentiveId &&
                        y.Uln == _changeOfCircumstanceInput.Uln)), Times.Once);
        }

        [Test]
        public async Task Then_learner_match_is_called_after_recalculating_earnings()
        {
            var sequence = new MockSequence();
            _mockOrchestrationContext.InSequence(sequence).Setup(
                x => x.CallActivityAsync("CalculateEarningsActivity",
                    It.Is<LearnerMatchInput>(y =>  y.ApprenticeshipIncentiveId == _changeOfCircumstanceInput.ApprenticeshipIncentiveId)));
            _mockOrchestrationContext.InSequence(sequence).Setup(x => x.CallActivityAsync("LearnerMatchAndUpdate",
                It.Is<LearnerMatchInput>(y => y.ApprenticeshipIncentiveId == _changeOfCircumstanceInput.ApprenticeshipIncentiveId)));

            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(
                x => x.CallActivityAsync("LearnerMatchAndUpdate",
                    It.Is<LearnerMatchInput>(y => y.ApprenticeshipIncentiveId == _changeOfCircumstanceInput.ApprenticeshipIncentiveId)), Times.Once);
        }

        [Test]
        public async Task Then_change_of_circumstances_is_not_triggered_when_there_is_no_potential_change()
        {
            _mockOrchestrationContext.Setup(x => x.CallActivityAsync<bool>("ApprenticeshipIncentiveHasPossibleChangeOrCircs", _changeOfCircumstanceInput.ApprenticeshipIncentiveId)).ReturnsAsync(false);

            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(x => x.CallActivityAsync("LearnerChangeOfCircumstanceActivity", It.IsAny<LearnerChangeOfCircumstanceInput>()), Times.Never);
            _mockOrchestrationContext.Verify(x => x.CallActivityAsync("CalculateEarningsActivity", It.IsAny<CalculateEarningsInput>()), Times.Never);
            _mockOrchestrationContext.Verify(x => x.CallActivityAsync("LearnerMatchAndUpdate", It.IsAny<LearnerMatchInput>()), Times.Never);
        }
    }
}