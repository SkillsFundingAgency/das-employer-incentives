using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Application.Commands.AddLegalEntity;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Application.UnitTests.AddLegalEntity.Validators
{
    public class WhenValidatingAddLegalEntityCommand
    {
        private AddLegalEntityCommandValidator _sut;
        
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sut = new AddLegalEntityCommandValidator();
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_accountId_has_a_default_value()
        {
            //Arrange
            var command = new AddLegalEntityCommand(default, _fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<long>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }
        [Test]
        public async Task Then_the_command_is_invalid_when_the_legalEntityId_has_a_default_value()
        {
            //Arrange
            var command = new AddLegalEntityCommand(_fixture.Create<long>(), default, _fixture.Create<string>(), _fixture.Create<long>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }
        [Test]
        public async Task Then_the_command_is_invalid_when_the_accountLegalEntityId_has_a_default_value()
        {
            //Arrange
            var command = new AddLegalEntityCommand(_fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<string>(), default);

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }
        [Test]
        public async Task Then_the_command_is_invalid_when_the_name_is_null()
        {
            //Arrange
            var command = new AddLegalEntityCommand(_fixture.Create<long>(), _fixture.Create<long>(), null,  _fixture.Create<long>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }
        [Test]
        public async Task Then_the_command_is_invalid_when_the_name_is_empty()
        {
            //Arrange
            var command = new AddLegalEntityCommand(_fixture.Create<long>(), _fixture.Create<long>(), string.Empty, _fixture.Create<long>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }
    }
}
