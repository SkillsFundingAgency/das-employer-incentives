using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SetSuccessfulLearnerMatch;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
{
    public class WhenSetSuccessfulLearnerMatch
    {
        private Fixture _fixture;
        private SetSuccessfulLearnerMatch _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            
            _sut = new SetSuccessfulLearnerMatch(_mockCommandDispatcher.Object, Mock.Of<ILogger<SetSuccessfulLearnerMatch>>());
        }

        [Test]
        public async Task Then_command_is_called_to_update_the_learner()
        {
            var input = _fixture.Create<SetSuccessfulLearnerMatchInput>();
            await _sut.Set(input);

            _mockCommandDispatcher.Verify(
                x => x.Send(
                    It.Is<SetSuccessfulLearnerMatchCommand>(p =>
                        p.ApprenticeshipIncentiveId == input.ApprenticeshipIncentiveId
                        && p.Succeeded == input.Succeeded
                        && p.Uln == input.Uln
                    ), CancellationToken.None), Times.Once);
        }
    }
}