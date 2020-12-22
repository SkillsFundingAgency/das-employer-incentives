using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CreateIncentive;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.CreateIncentive
{
    [TestFixture]
    public class WhenValidatingCreateIncentiveCommand
    {
        private CreateIncentiveCommandValidator _sut;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sut = new CreateIncentiveCommandValidator();
        }

        [Test]
        public async Task Then_the_command_is_valid_when_all_mandatory_values_are_supplied()
        {
            // Arrange
            var command = new CreateIncentiveCommand(_fixture.Create<long>(), _fixture.Create<long>(),
                Guid.NewGuid(), _fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<string>(),
                _fixture.Create<DateTime>(), _fixture.Create<long>(), _fixture.Create<DateTime>(),
                _fixture.Create<ApprenticeshipEmployerType>(), _fixture.Create<long>());

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(0);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_account_id_is_not_set()
        {
            // Arrange
            var command = new CreateIncentiveCommand(default, _fixture.Create<long>(),
                Guid.NewGuid(), _fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<string>(),
                _fixture.Create<DateTime>(), _fixture.Create<long>(), _fixture.Create<DateTime>(),
                _fixture.Create<ApprenticeshipEmployerType>(), _fixture.Create<long>());

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_account_legal_entity_id_is_not_set()
        {
            // Arrange
            var command = new CreateIncentiveCommand(_fixture.Create<long>(), default,
                Guid.NewGuid(), _fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<string>(),
                _fixture.Create<DateTime>(), _fixture.Create<long>(), _fixture.Create<DateTime>(),
                _fixture.Create<ApprenticeshipEmployerType>(), _fixture.Create<long>());

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_apprenticeship_id_is_not_set()
        {
            // Arrange
            var command = new CreateIncentiveCommand(_fixture.Create<long>(), _fixture.Create<long>(),
                Guid.NewGuid(), default, _fixture.Create<string>(), _fixture.Create<string>(),
                _fixture.Create<DateTime>(), _fixture.Create<long>(), _fixture.Create<DateTime>(),
                _fixture.Create<ApprenticeshipEmployerType>(), _fixture.Create<long>());

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_date_of_birth_is_not_set() 
        {
            // Arrange
            var command = new CreateIncentiveCommand(_fixture.Create<long>(), _fixture.Create<long>(),
                Guid.NewGuid(), _fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<string>(),
                default, _fixture.Create<long>(), _fixture.Create<DateTime>(),
                _fixture.Create<ApprenticeshipEmployerType>(), _fixture.Create<long>());

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_first_name_is_not_set() 
        {
            // Arrange
            var command = new CreateIncentiveCommand(_fixture.Create<long>(), _fixture.Create<long>(),
                Guid.NewGuid(), _fixture.Create<long>(), default, _fixture.Create<string>(),
                _fixture.Create<DateTime>(), _fixture.Create<long>(), _fixture.Create<DateTime>(),
                _fixture.Create<ApprenticeshipEmployerType>(), _fixture.Create<long>());

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_last_name_is_not_set()
        {
            // Arrange
            var command = new CreateIncentiveCommand(_fixture.Create<long>(), _fixture.Create<long>(),
                Guid.NewGuid(), _fixture.Create<long>(), _fixture.Create<string>(), default,
                _fixture.Create<DateTime>(), _fixture.Create<long>(), _fixture.Create<DateTime>(),
                _fixture.Create<ApprenticeshipEmployerType>(), _fixture.Create<long>());

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_incentive_application_apprenticeship_id_is_not_set()
        {
            // Arrange
            var command = new CreateIncentiveCommand(_fixture.Create<long>(), _fixture.Create<long>(),
                default, _fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<string>(),
                _fixture.Create<DateTime>(), _fixture.Create<long>(), _fixture.Create<DateTime>(),
                _fixture.Create<ApprenticeshipEmployerType>(), _fixture.Create<long>());

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_planned_start_date_is_not_set()
        {
            // Arrange
            var command = new CreateIncentiveCommand(_fixture.Create<long>(), _fixture.Create<long>(),
                Guid.NewGuid(), _fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<string>(),
                _fixture.Create<DateTime>(), _fixture.Create<long>(), default,
                _fixture.Create<ApprenticeshipEmployerType>(), _fixture.Create<long>());

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_ukprn_is_not_set()
        {
            // Arrange
            var command = new CreateIncentiveCommand(_fixture.Create<long>(), _fixture.Create<long>(),
                Guid.NewGuid(), _fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<string>(),
                _fixture.Create<DateTime>(), _fixture.Create<long>(), _fixture.Create<DateTime>(),
                _fixture.Create<ApprenticeshipEmployerType>(), default);

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_uln_is_not_set()
        {
            // Arrange
            var command = new CreateIncentiveCommand(_fixture.Create<long>(), _fixture.Create<long>(),
                Guid.NewGuid(), _fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<string>(),
                _fixture.Create<DateTime>(), default, _fixture.Create<DateTime>(),
                _fixture.Create<ApprenticeshipEmployerType>(), _fixture.Create<long>());

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }
    }
}
