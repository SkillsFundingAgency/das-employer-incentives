using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
{
    public class WhenMatchingAndUpdatingALearner
    {
        private Fixture _fixture;
        private LearnerMatchAndUpdate _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            
            _sut = new LearnerMatchAndUpdate(_mockCommandDispatcher.Object, Mock.Of<ILogger<LearnerMatchAndUpdate>>());
        }

        [Test]
        public async Task Then_command_is_called_to_match_the_learner()
        {
            var input = _fixture.Create<LearnerMatchInput>();
            await _sut.Create(input);

            _mockCommandDispatcher.Verify(
                x => x.Send(
                    It.Is<RefreshLearnerCommand>(p =>
                        p.ApprenticeshipIncentiveId == input.ApprenticeshipIncentiveId), CancellationToken.None), Times.Once);
        }
    }
}