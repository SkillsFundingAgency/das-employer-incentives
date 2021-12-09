using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
{
    public class WhenCalculatingEarnings
    {
        private Fixture _fixture;
        private CalculateEarningsActivity _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            
            _sut = new CalculateEarningsActivity(_mockCommandDispatcher.Object);
        }

        [Test]
        public async Task Then_command_is_called_to_match_the_learner()
        {
            var input = _fixture.Create<CalculateEarningsInput>();
            await _sut.Update(input);

            _mockCommandDispatcher.Verify(
                x => x.Send(
                    It.Is<CalculateEarningsCommand>(p =>
                        p.ApprenticeshipIncentiveId == input.ApprenticeshipIncentiveId), CancellationToken.None), Times.Once);
        }
    }
}