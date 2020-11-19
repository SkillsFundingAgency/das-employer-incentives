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
                    It.Is<CalculateEarningsInput>(y =>
                        y.ApprenticeshipIncentiveId == _changeOfCircumstanceInput.ApprenticeshipIncentiveId &&
                        y.Uln == _changeOfCircumstanceInput.Uln)));

            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(
                x => x.CallActivityAsync("CalculateEarningsActivity",
                    It.Is<CalculateEarningsInput>(y =>
                        y.ApprenticeshipIncentiveId == _changeOfCircumstanceInput.ApprenticeshipIncentiveId &&
                        y.Uln == _changeOfCircumstanceInput.Uln)), Times.Once);
        }
    }
}