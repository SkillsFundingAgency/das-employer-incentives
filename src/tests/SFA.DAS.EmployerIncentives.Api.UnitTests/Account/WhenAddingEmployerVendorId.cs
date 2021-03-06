using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.AddEmployerVendorIdForLegalEntity;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Account
{
    public class WhenAddingEmployerVendorId
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
                .Setup(m => m.Send(It.IsAny<AddEmployerVendorIdForLegalEntityCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Test]
        public async Task Then_a_AddEmployerVendorIdForLegalEntityCommand_command_is_dispatched()
        {
            // Arrange
            var accountLegalEntityId = _fixture.Create<string>();
            var request = _fixture.Create<AddEmployerVendorIdRequest>();

            // Act
            await _sut.AddEmployerVendorId(accountLegalEntityId, request);

            // Assert
            _mockCommandDispatcher
                .Verify(m => m.Send(It.Is<AddEmployerVendorIdForLegalEntityCommand>(c =>
                    c.HashedLegalEntityId == accountLegalEntityId &&
                    c.EmployerVendorId == request.EmployerVendorId),
                It.IsAny<CancellationToken>())
                , Times.Once);
        }

        [Test]
        public async Task Then_a_NoContent_response_is_returned()
        {
            // Arrange
            var accountLegalEntityId = _fixture.Create<string>();
            var request = _fixture.Create<AddEmployerVendorIdRequest>();

            // Act
            var actual = await _sut.AddEmployerVendorId(accountLegalEntityId, request) as NoContentResult;

            // Assert
            actual.Should().NotBeNull();
        }
    }
}