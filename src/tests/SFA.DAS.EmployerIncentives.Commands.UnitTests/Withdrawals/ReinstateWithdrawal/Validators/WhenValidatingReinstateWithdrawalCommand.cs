using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Types.Withdrawals;
using SFA.DAS.EmployerIncentives.Commands.Withdrawals.ReinstateWithdrawal;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.Withdrawals.ReinstateWithdrawal.Validators
{
    public class WhenValidatingReinstateWithdrawalCommand
    {
        private ReinstateWithdrawalCommandValidator _sut;
        
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sut = new ReinstateWithdrawalCommandValidator();
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_AccountLegalEntityId_has_a_default_value()
        {
            //Arrange
            var command = new ReinstateWithdrawalCommand(
                default, 
                _fixture.Create<long>(),
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<DateTime>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_ULN_has_a_default_value()
        {
            //Arrange
            var command = new ReinstateWithdrawalCommand(                
                _fixture.Create<long>(),
                default,
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<DateTime>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }
    }
}
