using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.SignLegalEntityAgreement;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.SignLegalEntityAgreement.Validators
{
    public class WhenValidatingSignLegalEntityAgreementCommand
    {
        private SignLegalEntityAgreementCommandValidator _sut;
        
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sut = new SignLegalEntityAgreementCommandValidator();
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_accountId_has_a_default_value()
        {
            //Arrange
            var command = new SignLegalEntityAgreementCommand(default, _fixture.Create<long>(), _fixture.Create<int>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_accountLegalEntityId_has_a_default_value()
        {
            //Arrange
            var command = new SignLegalEntityAgreementCommand(_fixture.Create<long>(), default, _fixture.Create<int>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_agreementVersion_has_a_default_value()
        {
            //Arrange
            var command = new SignLegalEntityAgreementCommand(_fixture.Create<long>(), _fixture.Create<long>(), default);

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }
    }
}
