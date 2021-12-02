using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.EmploymentCheck;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.EmploymentCheck
{
    public class WhenUpdatingEmploymentCheck
    {
        private EmploymentCheckController _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;
        private Mock<ICollectionCalendarService> _mockCollectionCalendarService;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            _mockCollectionCalendarService = new Mock<ICollectionCalendarService>();
            _fixture = new Fixture();
            _sut = new EmploymentCheckController(_mockCommandDispatcher.Object, _mockCollectionCalendarService.Object);
        }

        [Test]
        public async Task Then_a_UpdateEmploymentCheckCommand_command_is_dispatched()
        {
            // Arrange
            var request = _fixture.Create<UpdateEmploymentCheckRequest>();

            // Act
            await _sut.Update(request);

            // Assert
            _mockCommandDispatcher
                .Verify(m => m.Send(It.Is<UpdateEmploymentCheckCommand>(c =>
                    c.CorrelationId == request.CorrelationId &&
                    c.Result == Map(request.Result) &&
                    c.DateChecked == request.DateChecked),
                It.IsAny<CancellationToken>())
                , Times.Once);
        }

        [Test]
        public async Task Then_an_Ok_response_is_returned()
        {
            // Arrange
            var request = _fixture.Create<UpdateEmploymentCheckRequest>();

            // Act
            var actual = await _sut.Update(request) as OkObjectResult;

            // Assert
            actual.Should().NotBeNull();
            actual.Value.Should().Be($"/employmentchecks/{request.CorrelationId}");
        }

        private EmploymentCheckResultType Map(string result)
        {
            return result.ToLower() switch
            {
                "employed" => EmploymentCheckResultType.Employed,
                "notemployed" => EmploymentCheckResultType.NotEmployed,
                "hmrcunknown" => EmploymentCheckResultType.HMRCUnknown,
                "noninofound" => EmploymentCheckResultType.NoNINOFound,
                _ => EmploymentCheckResultType.NoAccountFound,
            };
        }
    }
}
