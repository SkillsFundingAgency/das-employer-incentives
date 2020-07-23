using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.AddLegalEntity;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Account
{
    public class WhenAddLegalEntity
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
                .Setup(m => m.Send(It.IsAny<AddLegalEntityCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Test]
        public async Task Then_a_AddLegalEntityCommand_command_is_dispatched()
        {
            // Arrange
            var request = _fixture.Create<AddLegalEntityRequest>();
            var accountId = _fixture.Create<long>();

            // Act
            await _sut.AddLegalEntity(accountId, request);

            // Assert
            _mockCommandDispatcher
                .Verify(m => m.Send(It.Is<AddLegalEntityCommand>(c => 
                    c.AccountId == accountId && 
                    c.AccountLegalEntityId == request.AccountLegalEntityId &&
                    c.LegalEntityId == request.LegalEntityId &&
                    c.Name == request.OrganisationName), 
                It.IsAny<CancellationToken>())
                ,Times.Once);                
        }

        [Test]
        public async Task Then_a_Created_response_is_returned()
        {
            // Arrange
            var request = _fixture.Create<AddLegalEntityRequest>();
            var accountId = _fixture.Create<long>();

            var expected = new LegalEntityDto { 
                AccountId = accountId,
                AccountLegalEntityId = request.AccountLegalEntityId,
                LegalEntityId = request.LegalEntityId,
                LegalEntityName = request.OrganisationName
             };

            // Act
            var actual = await _sut.AddLegalEntity(accountId, request) as CreatedResult;

            // Assert
            actual.Should().NotBeNull();
            actual.Value.Should().BeEquivalentTo(expected);
        }
    }
}