using System;
using System.Linq;
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
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Commands;

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
            var incentiveLearnerIdentifiers = _fixture.CreateMany<IncentiveLearnerIdentifier>(5).ToList();
            var request = new RecalculateEarningsRequest { IncentiveLearnerIdentifiers = incentiveLearnerIdentifiers };
            _commandDispatcher.Setup(
                x => x.Send(It.IsAny<RecalculateEarningsCommand>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            // Act
            var actual = await _sut.RecalculateEarnings(request) as NoContentResult;

            // Assert
            actual.Should().NotBeNull();
            for(var index = 0; index < incentiveLearnerIdentifiers.Count; index++)
            {
                _commandDispatcher.Verify(x => x.Send(It.Is<RecalculateEarningsCommand>(
                    y => y.IncentiveLearnerIdentifiers.FirstOrDefault(
                        z => z.AccountLegalEntityId == incentiveLearnerIdentifiers[index].AccountLegalEntityId
                        && z.ULN == incentiveLearnerIdentifiers[index].ULN) != null),
                        It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        [Test]
        public async Task Then_a_bad_request_response_is_returned_for_invalid_account_legal_entity_and_ULN()
        {
            // Arrange
            var incentiveLearnerIdentifiers = _fixture.CreateMany<IncentiveLearnerIdentifier>(5).ToList();
            var request = new RecalculateEarningsRequest { IncentiveLearnerIdentifiers = incentiveLearnerIdentifiers };
            _commandDispatcher.Setup(
                x => x.Send(It.IsAny<RecalculateEarningsCommand>(), It.IsAny<CancellationToken>())).Throws(new ArgumentException("Invalid details"));

            // Act
            var actual = await _sut.RecalculateEarnings(request) as BadRequestObjectResult;

            // Assert
            actual.Should().NotBeNull();
            actual.Value.Should().Be("Invalid details");
        }
    }
}
