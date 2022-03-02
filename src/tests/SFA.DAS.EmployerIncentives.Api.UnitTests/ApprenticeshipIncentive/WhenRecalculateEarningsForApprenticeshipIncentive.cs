using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetApprenticeshipIncentivesForAccountLegalEntity;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.ApprenticeshipIncentive
{
    public class WhenRecalculateEarningsForApprenticeshipIncentive
    {
        private ApprenticeshipIncentiveCommandController _sut;
        private Mock<ICommandDispatcher> _commandDispatcher;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _commandDispatcher = new Mock<ICommandDispatcher>();
            _fixture = new Fixture();
            _sut = new ApprenticeshipIncentiveCommandController(_commandDispatcher.Object);
        }

        [Test]
        public async Task Then_the_command_to_recalculate_earnings_is_dispatched()
        {
            // Arrange
            var incentiveLearnerIdentifiers = _fixture.CreateMany<IncentiveLearnerIdentifierDto>(5).ToList();
            var request = new RecalculateEarningsRequest { IncentiveLearnerIdentifiers = incentiveLearnerIdentifiers };
            _commandDispatcher
                .Setup(x => x.Send(It.Is<RecalculateEarningsCommand>(y =>
                    y.IncentiveLearnerIdentifiers.Equals(incentiveLearnerIdentifiers)), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            // Act
            var actual = await _sut.RecalculateEarnings(request) as NoContentResult;

            // Assert
            actual.Should().NotBeNull();
            _commandDispatcher.Verify(x => x.Send(It.Is<RecalculateEarningsCommand>(y =>
                y.IncentiveLearnerIdentifiers.Equals(incentiveLearnerIdentifiers)), It.IsAny<CancellationToken>()), Times.Once());
        }
    }
}
