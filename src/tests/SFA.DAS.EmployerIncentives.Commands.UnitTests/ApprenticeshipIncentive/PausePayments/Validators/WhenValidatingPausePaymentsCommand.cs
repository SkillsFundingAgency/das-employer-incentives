using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.PausePayments;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.PausePayments.Validators
{
    public class WhenValidatingPausePaymentsCommand
    {
        private PausePaymentsCommandValidator _sut;
        private long _accountLegalEntityId;
        private long _uln;
        private string _serviceRequestId;
        private DateTime _dateServiceRequestTaskCreated;
        private string _decisionReferenceNumber;
        private PausePaymentsAction? _action;
        private Func<PausePaymentsCommand> CreateCommand => () => new PausePaymentsCommand(_uln, _accountLegalEntityId, _serviceRequestId, _decisionReferenceNumber, _dateServiceRequestTaskCreated, _action);

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _uln = _fixture.Create<long>();
            _accountLegalEntityId = _fixture.Create<long>();
            _serviceRequestId = _fixture.Create<string>();
            _decisionReferenceNumber = _fixture.Create<string>();
            _dateServiceRequestTaskCreated = _fixture.Create<DateTime>();
            _action = _fixture.Create<PausePaymentsAction>();

            _sut = new PausePaymentsCommandValidator();
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_ULN_has_a_default_value()
        {
            //Arrange
            _uln = default;
            var command = CreateCommand();

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_AccountLegalEntityId_has_a_default_value()
        {
            //Arrange
            _accountLegalEntityId = default;
            var command = CreateCommand();

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_ServiceRequestId_has_a_default_value()
        {
            //Arrange
            _serviceRequestId = default;
            var command = CreateCommand();

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_DateServiceRequestTaskCreated_has_a_default_value()
        {
            //Arrange
            _dateServiceRequestTaskCreated = default;
            var command = CreateCommand();

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("    ")]
        public async Task Then_the_command_is_invalid_when_the_DecisionReferenceNumber_has_an_empty_value(string decisionReferenceNumber)
        {
            //Arrange
            _decisionReferenceNumber = decisionReferenceNumber;
            var command = CreateCommand();

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_PausedPaymentsAction_has_a_default_value()
        {
            //Arrange
            _action = default;
            var command = CreateCommand();

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }
    }
}
