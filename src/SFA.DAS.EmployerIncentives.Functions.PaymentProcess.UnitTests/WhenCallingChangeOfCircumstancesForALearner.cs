using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.LearnerChangeOfCircumstance;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
{
    public class WhenCallingChangeOfCircumstancesForALearner
    {
        private Fixture _fixture;
        private LearnerChangeOfCircumstanceActivity _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            
            _sut = new LearnerChangeOfCircumstanceActivity(_mockCommandDispatcher.Object, Mock.Of<ILogger<LearnerChangeOfCircumstanceActivity>>());
        }

        [Test]
        public async Task Then_command_is_called_to_match_the_learner()
        {
            var input = _fixture.Create<LearnerChangeOfCircumstanceInput>();
            await _sut.Update(input);

            _mockCommandDispatcher.Verify(
                x => x.Send(
                    It.Is<LearnerChangeOfCircumstanceCommand>(p =>
                        p.ApprenticeshipIncentiveId == input.ApprenticeshipIncentiveId), CancellationToken.None), Times.Once);
        }
    }
}