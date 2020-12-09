using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Withdrawls.EmployerWithdrawl;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.Withdrawls.EmployerWithdrawl.Validators
{
    public class WhenValidatingEmployerWithdrawlCommand
    {
        private EmployerWithdrawlCommandValidator _sut;
        
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sut = new EmployerWithdrawlCommandValidator();
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_AccountLegalEntityId_has_a_default_value()
        {
            //Arrange
            var command = new EmployerWithdrawlCommand(
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
            var command = new EmployerWithdrawlCommand(                
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
