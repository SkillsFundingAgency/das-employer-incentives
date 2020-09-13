using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.Create;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.Create.Validators
{
    public class WhenValidatingCreateCommand
    {
        private CreateCommandValidator _sut;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sut = new CreateCommandValidator();
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_incentive_application_id_has_a_default_value()
        {
            //Arrange
            var command = new CreateCommand(_fixture.Create<long>(), default);

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_account_id_has_a_default_value()
        {
            //Arrange
            var command = new CreateCommand(default, _fixture.Create<Guid>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }
    }
}
