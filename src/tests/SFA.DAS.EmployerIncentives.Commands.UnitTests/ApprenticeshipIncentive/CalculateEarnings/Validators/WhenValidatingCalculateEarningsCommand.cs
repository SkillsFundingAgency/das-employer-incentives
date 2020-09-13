using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CalculateEarnings;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.CalculateEarnings.Validators
{
    public class WhenValidatingCalculateEarningsCommand
    {
        private CalculateEarningsCommandValidator _sut;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sut = new CalculateEarningsCommandValidator();
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_apprenticeship_incentive_id_has_a_default_value()
        {
            //Arrange
            var command = new CalculateEarningsCommand(default, _fixture.Create<long>(), _fixture.Create<long>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_account_id_has_a_default_value()
        {
            //Arrange
            var command = new CalculateEarningsCommand(_fixture.Create<Guid>(), default, _fixture.Create<long>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_apprenticeship_id_has_a_default_value()
        {
            //Arrange
            var command = new CalculateEarningsCommand(_fixture.Create<Guid>(), _fixture.Create<long>(), default );

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }
    }
}
