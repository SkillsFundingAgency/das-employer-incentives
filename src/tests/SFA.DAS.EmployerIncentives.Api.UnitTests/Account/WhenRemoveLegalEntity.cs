using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Commands.RemoveLegalEntity;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Account
{
    public class WhenRemoveLegalEntity
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
                .Setup(m => m.Send(It.IsAny<RemoveLegalEntityCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Test]
        public async Task Then_a_RemoveLegalEntityCommand_command_is_dispatched()
        {
            // Arrange
            var accountLegalEntityId = _fixture.Create<long>();
            var accountId = _fixture.Create<long>();

            // Act
            await _sut.RemoveLegalEntity(accountId, accountLegalEntityId);

            // Assert
            _mockCommandDispatcher
                .Verify(m => m.Send(It.Is<RemoveLegalEntityCommand>(c => 
                    c.AccountId == accountId && 
                    c.AccountLegalEntityId == accountLegalEntityId), 
                It.IsAny<CancellationToken>())
                ,Times.Once);                
        }

        [Test]
        public async Task Then_an_OK_response_is_returned()
        {
            // Arrange
            var accountLegalEntityId = _fixture.Create<long>();
            var accountId = _fixture.Create<long>();

            // Act
            var actual = await _sut.RemoveLegalEntity(accountId, accountLegalEntityId) as OkResult;

            // Assert
            actual.Should().NotBeNull();
        }
    }
}