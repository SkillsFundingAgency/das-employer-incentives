using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.SignLegalEntityAgreement;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Account
{
    public class WhenSigningAgreement
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
                .Setup(m => m.Send(It.IsAny<SignLegalEntityAgreementCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Test]
        public async Task Then_a_RemoveLegalEntityCommand_command_is_dispatched()
        {
            // Arrange
            var accountLegalEntityId = _fixture.Create<long>();
            var accountId = _fixture.Create<long>();
            var request = _fixture.Create<SignedAgreement>();

            // Act
            await _sut.SignAgreement(accountId, accountLegalEntityId, request);

            // Assert
            _mockCommandDispatcher
                .Verify(m => m.Send(It.Is<SignLegalEntityAgreementCommand>(c => 
                    c.AccountId == accountId && 
                    c.AccountLegalEntityId == accountLegalEntityId &&
                    c.AgreementVersion == request.AgreementVersion), 
                It.IsAny<CancellationToken>())
                ,Times.Once);                
        }

        [Test]
        public async Task Then_a_NoContent_response_is_returned()
        {
            // Arrange
            var accountLegalEntityId = _fixture.Create<long>();
            var accountId = _fixture.Create<long>();
            var request = _fixture.Create<SignedAgreement>();

            // Act
            var actual = await _sut.SignAgreement(accountId, accountLegalEntityId, request) as NoContentResult;

            // Assert
            actual.Should().NotBeNull();
        }
    }
}