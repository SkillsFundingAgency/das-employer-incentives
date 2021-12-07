using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.EmploymentCheck;
using SFA.DAS.EmployerIncentives.Commands.RefreshLegalEntities;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Job
{
    [TestFixture]
    public class WhenRefreshEmploymentChecks
    {
        private JobCommandController _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            _fixture = new Fixture();
            _sut = new JobCommandController(_mockCommandDispatcher.Object);

            _mockCommandDispatcher
                .Setup(m => m.Send(It.IsAny<RefreshLegalEntitiesCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Test]
        public async Task Then_a_refresh_employment_checks_command_is_dispatched()
        {
            // Arrange
            var request = _fixture.Build<JobRequest>().With(r => r.Type, JobType.RefreshEmploymentChecks).Create();

            // Act
            await _sut.AddJob(request);

            // Assert
            _mockCommandDispatcher.Verify(m => m.Send(It.IsAny<RefreshEmploymentChecksCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Then_a_NoContent_response_is_returned()
        {
            // Arrange
            var request = _fixture.Build<JobRequest>().With(r => r.Type, JobType.RefreshEmploymentChecks).Create();

            // Act
            var actual = await _sut.AddJob(request) as NoContentResult;

            // Assert
            actual.Should().NotBeNull();
        }
    }
}
