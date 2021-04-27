using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApplicationApprenticeships;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApplicationApprenticeships.Validators
{
    [TestFixture]
    public class WhenValidatingAddApplicationApprenticeshipsCommand
    {
        private AddApplicationApprenticeshipsCommandValidator _sut;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _sut = new AddApplicationApprenticeshipsCommandValidator();
        }

        [Test]
        public async Task Then_the_command_is_invalid_if_the_application_id_is_not_set()
        {
            // Arrange
            var command =
                new AddApplicationApprenticeshipsCommand(default, _fixture.Create<long>(),
                    _fixture.CreateMany<IncentiveApplicationApprenticeshipDto>(5));

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_if_the_account_id_is_not_set()
        {
            // Arrange
            var command =
                new AddApplicationApprenticeshipsCommand(_fixture.Create<Guid>(), default,
                    _fixture.CreateMany<IncentiveApplicationApprenticeshipDto>(5));

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_if_no_apprenticeship_ids_are_set()
        {
            // Arrange
            var command =
                new AddApplicationApprenticeshipsCommand(_fixture.Create<Guid>(), _fixture.Create<long>(), default);

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }
    }
}
