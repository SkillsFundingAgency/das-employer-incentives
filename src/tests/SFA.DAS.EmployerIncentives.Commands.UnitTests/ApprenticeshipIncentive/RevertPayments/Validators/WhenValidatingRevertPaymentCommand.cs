using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.RevertPayments;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.RevertPayments.Validators
{
    [TestFixture]
    public class WhenValidatingRevertPaymentCommand
    {
        private Fixture _fixture;
        private RevertPaymentCommandValidator _sut;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _sut = new RevertPaymentCommandValidator();
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_payment_id_is_not_specified()
        {
            // Arrange
            var command = new RevertPaymentCommand(Guid.Empty, _fixture.Create<string>(), _fixture.Create<string>(),
                _fixture.Create<DateTime>());

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("  ")]
        public async Task Then_the_command_is_invalid_when_the_service_request_id_is_not_specified(string serviceRequestId)
        {
            // Arrange
            var command = new RevertPaymentCommand(_fixture.Create<Guid>(), serviceRequestId, _fixture.Create<string>(),
                _fixture.Create<DateTime>());

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("  ")]
        public async Task Then_the_command_is_invalid_when_the_decision_reference_is_not_specified(string decisionReference)
        {
            // Arrange
            var command = new RevertPaymentCommand(_fixture.Create<Guid>(), _fixture.Create<string>(), decisionReference,
                _fixture.Create<DateTime>());

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_service_request_created_date_is_not_specified()
        {
            // Arrange
            var command = new RevertPaymentCommand(_fixture.Create<Guid>(), _fixture.Create<string>(), _fixture.Create<string>(),
                default);

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }
    }
}
