using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Application.Commands.RemoveLegalEntity;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Application.UnitTests.RemoveLegalEntity.Validators
{
    public class WhenValidatingRemoveLegalEntityCommand
    {
        private RemoveLegalEntityCommandValidator _sut;
        
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sut = new RemoveLegalEntityCommandValidator();
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_accountId_has_a_default_value()
        {
            //Arrange
            var command = new RemoveLegalEntityCommand(default, _fixture.Create<long>(), _fixture.Create<long>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }
        [Test]
        public async Task Then_the_command_is_invalid_when_the_legalEntityId_has_a_default_value()
        {
            //Arrange
            var command = new RemoveLegalEntityCommand(_fixture.Create<long>(), default, _fixture.Create<long>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }
        [Test]
        public async Task Then_the_command_is_invalid_when_the_accountLegalEntityId_has_a_default_value()
        {
            //Arrange
            var command = new RemoveLegalEntityCommand(_fixture.Create<long>(), _fixture.Create<long>(), default);

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }        
    }
}
