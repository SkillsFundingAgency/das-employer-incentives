using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.ApplicationApprenticeships;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApplicationApprenticeships.Validators
{
    [TestFixture]
    public class WhenValidatingRemoveApplicationApprenticeshipCommand
    {
        private RemoveApplicationApprenticeshipCommandValidator _sut;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _sut = new RemoveApplicationApprenticeshipCommandValidator();
        }

        [Test]
        public async Task Then_the_command_is_invalid_if_the_application_id_is_not_set()
        {
            // Arrange
            var command =
                new RemoveApplicationApprenticeshipCommand(default, _fixture.Create<long>());

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_if_the_apprenticeship_id_is_not_set()
        {
            // Arrange
            var command =
                new RemoveApplicationApprenticeshipCommand(_fixture.Create<Guid>(), default);

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

    }
}
