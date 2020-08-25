using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.SubmitIncentiveApplication;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Application
{
    public class WhenSubmittingAnApplication
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
            var request = _fixture.Create<SubmitIncentiveApplicationRequest>();

            // Act
            await _sut.SubmitIncentiveApplication(request);

            // Assert
            _mockCommandDispatcher
                .Verify(m => m.Send(It.Is<SubmitIncentiveApplicationCommand>(c =>
                    c.IncentiveApplicationId == request.IncentiveApplicationId &&
                    c.AccountId == request.AccountId &&
                    c.DateSubmitted == request.DateSubmitted &&
                    c.SubmittedByEmail == request.SubmittedByEmail),
                It.IsAny<CancellationToken>())
                , Times.Once);
        }

        [Test]
        public async Task Then_a_success_response_is_returned()
        {
            // Arrange
            var request = _fixture.Create<SubmitIncentiveApplicationRequest>();

            // Act
            var result = await _sut.SubmitIncentiveApplication(request) as OkResult;

            // Assert
            result.Should().NotBeNull();
        }
    }
}
