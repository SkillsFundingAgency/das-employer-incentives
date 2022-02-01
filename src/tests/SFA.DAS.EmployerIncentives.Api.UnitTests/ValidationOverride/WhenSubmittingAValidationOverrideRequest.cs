using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.ValidationOverrides;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.ValidationOverride
{
    public class WhenSubmittingAValidationOverrideRequest
    {
        private ValidationOverrideController _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            _fixture = new Fixture();
            _sut = new ValidationOverrideController(_mockCommandDispatcher.Object);

            _mockCommandDispatcher
                .Setup(m => m.SendMany(It.IsAny<IEnumerable<ICommand>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Test]
        public async Task Then_ValidationOverrideCommands_are_dispatched_when_the_ValidationOverrideRequest_is_Submitted()
        {
            // Arrange
            var request = new ValidationOverrideRequest();
            var override1 = _fixture.Build<Types.ValidationOverride>().Create();
            var override2 = _fixture.Build<Types.ValidationOverride>().Create();
            request.ValidationOverrides = new List<Types.ValidationOverride>() { override1, override2 }.ToArray();

            var expectedCommand1 = new ValidationOverrideCommand(override1.AccountLegalEntityId, override1.ULN, override1.ServiceRequest.TaskId, 
                override1.ServiceRequest.DecisionReference, override1.ServiceRequest.TaskCreatedDate, 
                Map(override1.ValidationSteps));

            var expectedCommand2 = new ValidationOverrideCommand(override2.AccountLegalEntityId, override2.ULN, override2.ServiceRequest.TaskId,
                override2.ServiceRequest.DecisionReference, override2.ServiceRequest.TaskCreatedDate,
                Map(override2.ValidationSteps));

            var sentCommands = new List<ValidationOverrideCommand>();
            _mockCommandDispatcher.Setup(m => m.SendMany(It.IsAny<IEnumerable<ICommand>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<ICommand>, CancellationToken>((c, t) => sentCommands = c.Select(c => c  as ValidationOverrideCommand).ToList());                

            // Act
            await _sut.Add(request);

            // Assert
            _mockCommandDispatcher
                .Verify(m => m.SendMany(It.IsAny<IEnumerable<ICommand>>(),
                It.IsAny<CancellationToken>())
                , Times.Once);

            sentCommands.Single(c => c.AccountLegalEntityId == override1.AccountLegalEntityId).Should().BeEquivalentTo(expectedCommand1, opts => opts.Excluding(x => x.Log));
            sentCommands.Single(c => c.AccountLegalEntityId == override2.AccountLegalEntityId).Should().BeEquivalentTo(expectedCommand2, opts => opts.Excluding(x => x.Log));
        }

        private IEnumerable<ValidationOverrideStep> Map(ValidationStep[] validationStep)
        {
            return validationStep.Select(s => Map(s)).AsEnumerable();
        }

        private ValidationOverrideStep Map(ValidationStep validationStep)
        {
            return new ValidationOverrideStep(validationStep.ValidationType.ToString(), validationStep.ExpiryDate, validationStep.Remove ?? false);
        }

        [Test]
        public async Task Then_an_accepted_response_is_returned_when_the_ValidationOverrideRequest_is_Submitted()
        {
            // Arrange
            var request = _fixture.Create<ValidationOverrideRequest>();

            // Act
            var actual = await _sut.Add(request) as AcceptedResult;

            // Assert
            actual.Should().NotBeNull();
        }
    }
}