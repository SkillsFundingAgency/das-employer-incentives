using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Withdrawals.EmployerWithdrawal;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.Withdrawals.EmployerWithdrawal.Validators
{
    public class WhenValidatingEmployerWithdrawalCommand
    {
        private EmployerWithdrawalCommandValidator _sut;
        
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sut = new EmployerWithdrawalCommandValidator();
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_AccountLegalEntityId_has_a_default_value()
        {
            //Arrange
            var command = new EmployerWithdrawalCommand(
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
            var command = new EmployerWithdrawalCommand(                
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
