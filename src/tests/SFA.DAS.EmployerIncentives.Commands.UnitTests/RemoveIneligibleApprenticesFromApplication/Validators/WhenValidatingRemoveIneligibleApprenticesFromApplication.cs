using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.RemoveIneligibleApprenticesFromApplication;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.RemoveIneligibleApprenticesFromApplication.Validators
{
    public class WhenValidatingRemoveIneligibleApprenticesFromApplication
    {
        private RemoveIneligibleApprenticesFromApplicationCommandValidator _sut;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sut = new RemoveIneligibleApprenticesFromApplicationCommandValidator();
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_incentive_application_id_has_a_default_value()
        {
            //Arrange
            var command = new RemoveIneligibleApprenticesFromApplicationCommand(default, _fixture.Create<long>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_account_id_has_a_default_value()
        {
            //Arrange
            var command = new RemoveIneligibleApprenticesFromApplicationCommand(_fixture.Create<Guid>(), default);

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_valid_when_all_values_are_provided()
        {
            //Arrange
            var command = new RemoveIneligibleApprenticesFromApplicationCommand(_fixture.Create<Guid>(), _fixture.Create<long>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(0);
            result.IsValid().Should().BeTrue();
        }
    }
}
