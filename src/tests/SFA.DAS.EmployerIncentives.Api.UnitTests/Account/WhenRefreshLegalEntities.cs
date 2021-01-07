using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.RefreshLegalEntities;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Account
{
    [TestFixture]
    public class WhenRefreshLegalEntities
    {
        private AccountCommandController _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            _fixture = new Fixture();
            _sut = new AccountCommandController(_mockCommandDispatcher.Object);

            _mockCommandDispatcher
                .Setup(m => m.Send(It.IsAny<RefreshLegalEntitiesCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Test]
        public async Task Then_a_AddLegalEntityCommand_command_is_dispatched()
        {
            // Arrange
            var request = _fixture.Create<RefreshLegalEntitiesRequest>();
            var accountId = _fixture.Create<long>();

            // Act
            await _sut.RefreshLegalEntities(request);

            // Assert
            _mockCommandDispatcher
                .Verify(m => m.Send(It.Is<RefreshLegalEntitiesCommand>(c =>
                    c.PageNumber == request.PageNumber 
                    && c.PageSize == request.PageSize 
                    && c.AccountLegalEntities == request.AccountLegalEntities),
                It.IsAny<CancellationToken>())
                , Times.Once);
        }
    }
}
