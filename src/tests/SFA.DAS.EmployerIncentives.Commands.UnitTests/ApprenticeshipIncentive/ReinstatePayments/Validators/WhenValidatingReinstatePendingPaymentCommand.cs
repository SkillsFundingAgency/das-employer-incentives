using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.ReinstatePayments;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.ReinstatePayments.Validators
{
    [TestFixture]
    public class WhenValidatingReinstatePendingPaymentCommand
    {
        private Fixture _fixture;
        private ReinstatePendingPaymentCommandValidator _sut;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _sut = new ReinstatePendingPaymentCommandValidator();
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_payment_id_is_not_supplied()
        {
            // Arrange
            var command = new ReinstatePendingPaymentCommand(Guid.Empty, _fixture.Create<ReinstatePaymentRequest>());

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
            var command = new ReinstatePendingPaymentCommand(_fixture.Create<Guid>(), 
                new ReinstatePaymentRequest(serviceRequestId, _fixture.Create<string>(), _fixture.Create<DateTime>(), _fixture.Create<string>())
            );

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
            var command = new ReinstatePendingPaymentCommand(_fixture.Create<Guid>(),
                new ReinstatePaymentRequest(_fixture.Create<string>(), decisionReference, _fixture.Create<DateTime>(), _fixture.Create<string>())
            );

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }
        
        [Test]
        public async Task Then_the_command_is_invalid_when_the_service_request_created_date_is_not_specified()
        {
            // Arrange
            var command = new ReinstatePendingPaymentCommand(_fixture.Create<Guid>(),
                new ReinstatePaymentRequest(_fixture.Create<string>(), _fixture.Create<string>(), DateTime.MinValue, _fixture.Create<string>())
            );

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("  ")]
        public async Task Then_the_command_is_invalid_when_the_process_is_not_specified(string process)
        {
            // Arrange
            var command = new ReinstatePendingPaymentCommand(_fixture.Create<Guid>(),
                new ReinstatePaymentRequest(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<DateTime>(), process)
            );

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }
    }
}
