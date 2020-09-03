using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.IncentiveApplicationCalculateClaim;
using SFA.DAS.EmployerIncentives.Commands.Types.Application;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.IncentiveApplicationCalculateClaim.Validators
{
    public class WhenValidatingCalculateClaim
    {
        private CalculateClaimCommandValidator _sut;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sut = new CalculateClaimCommandValidator();
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_incentive_application_id_has_a_default_value()
        {
            //Arrange
            var command = new CalculateClaimCommand(default, _fixture.Create<Guid>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_account_id_has_a_default_value()
        {
            //Arrange
            var command = new CalculateClaimCommand(_fixture.Create<long>(), default);

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }
    }
}
