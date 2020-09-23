using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Api.Types;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Commands.UpdateVrfCaseStatusForLegalEntity;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Account
{
    public class WhenRefreshVendorRegistrationCaseStatus
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
                .Setup(m => m.Send(It.IsAny<UpdateVendorRegistrationCaseStatusCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Test]
        public async Task Then_a_UpdateVendorRegistrationCaseStatusCommand_command_is_dispatched()
        {
            // Arrange
            var accountLegalEntityId = _fixture.Create<string>();
            var request = _fixture.Create<UpdateVendorRegistrationCaseStatusRequest>();

            // Act
            await _sut.UpdateVendorRegistrationCaseStatus(accountLegalEntityId, request);

            // Assert
            _mockCommandDispatcher
                .Verify(m => m.Send(It.Is<UpdateVendorRegistrationCaseStatusCommand>(c =>
                    c.HashedLegalEntityId == accountLegalEntityId &&
                    c.CaseStatusLastUpdatedDate == request.CaseStatusLastUpdatedDate &&
                    c.CaseId == request.CaseId &&
                    c.VendorId == request.VendorId &&
                    c.Status == request.Status),
                It.IsAny<CancellationToken>())
                , Times.Once);
        }

        [Test]
        public async Task Then_a_NoContent_response_is_returned()
        {
            // Arrange
            var accountLegalEntityId = _fixture.Create<long>();
            var request = _fixture.Create<UpdateVendorRegistrationFormRequest>();

            // Act
            var actual = await _sut.UpdateVendorRegistrationForm(accountLegalEntityId, request) as NoContentResult;

            // Assert
            actual.Should().NotBeNull();
        }
    }
}