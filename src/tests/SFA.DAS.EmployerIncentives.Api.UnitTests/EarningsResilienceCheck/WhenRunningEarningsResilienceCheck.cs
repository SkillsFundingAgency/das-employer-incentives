using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Queries.EarningsResilienceCheck;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.EarningsResilienceCheck
{
    [TestFixture]
    public class WhenRunningEarningsResilienceCheck
    {
        private EarningsResilienceCommandController _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;

        [SetUp]
        public void Setup()
        {
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            _sut = new EarningsResilienceCommandController(_mockCommandDispatcher.Object);

            _mockCommandDispatcher
                .Setup(m => m.Send(It.IsAny<EarningsResilienceApplicationsCheckCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Test]
        public async Task Then_an_earning_resilience_check_command_is_dispatched()
        {
            // Act
            await _sut.CheckApplications();

            // Assert
            _mockCommandDispatcher.Verify(m => m.Send(It.IsAny<EarningsResilienceApplicationsCheckCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
