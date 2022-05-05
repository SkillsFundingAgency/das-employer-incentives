using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.RecalculateEarnings;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.RecalculateEarnings.Validators
{
    [TestFixture]
    public class WhenValidatingRecalculateEarningsCommand
    {
        private RecalculateEarningsCommandValidator _sut;
        
        [SetUp]
        public void Arrange()
        {
            _sut = new RecalculateEarningsCommandValidator();
        }

        [Test]
        public async Task Then_validation_fails_if_the_learner_identifiers_are_not_set()
        {
            // Arrange
            var command = new RecalculateEarningsCommand(null);

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_validation_fails_if_the_learner_identifiers_are_empty()
        {
            // Arrange
            var command = new RecalculateEarningsCommand(new List<IncentiveLearnerIdentifier>());

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }
        
        [Test]
        public async Task Then_validation_fails_if_the_learner_identifier_account_legal_entity_is_not_set()
        {
            // Arrange
            var identifiers = new List<IncentiveLearnerIdentifier>
            {
                new IncentiveLearnerIdentifier(accountLegalEntityId: 0, uln: 1234)
            };
            var command = new RecalculateEarningsCommand(identifiers);

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_validation_fails_if_the_learner_identifier_ULN_is_not_set()
        {
            // Arrange
            var identifiers = new List<IncentiveLearnerIdentifier>
            {
                new IncentiveLearnerIdentifier(accountLegalEntityId: 1234, uln: 0)
            };
            var command = new RecalculateEarningsCommand(identifiers);

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_validation_passes_for_a_valid_command()
        {
            // Arrange
            var identifiers = new List<IncentiveLearnerIdentifier>
            {
                new IncentiveLearnerIdentifier(accountLegalEntityId: 1234, uln: 11112222),
                new IncentiveLearnerIdentifier(accountLegalEntityId: 2345, uln: 22223333)
            };
            var command = new RecalculateEarningsCommand(identifiers);

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(0);
        }
    }
}
