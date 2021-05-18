using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.RemoveIneligibleApprenticesFromApplication;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Application
{
    public class WhenRemoveIneligibleApprentices
    {
        private ApplicationCommandController _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            _fixture = new Fixture();
            _sut = new ApplicationCommandController(_mockCommandDispatcher.Object);
        }

        [Test]
        public async Task Then_a_submit_application_command_is_dispatched()
        {
            // Arrange
            var request = _fixture.Build<PatchIncentiveApplicationRequest>()
                .With(x => x.Action, "RemoveIneligibleApprentices").Create();

            // Act
            await _sut.SubmitIncentiveApplication(request);

            // Assert
            _mockCommandDispatcher
                .Verify(m => m.Send(It.Is<RemoveIneligibleApprenticesFromApplicationCommand>(c =>
                    c.IncentiveApplicationId == request.IncentiveApplicationId &&
                    c.AccountId == request.AccountId),
                It.IsAny<CancellationToken>())
                , Times.Once);
        }

    }
}
