using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.AddEmployerVendorIdForLegalEntity;
using SFA.DAS.EmployerIncentives.Commands.UpdateVrfCaseStatusForLegalEntity;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.AddEmployerVendorIdForLegalEntity.Validators
{
    public class WhenValidatingAddEmployerVendorIdForLegalEntityCommandValidator
    {
        private AddEmployerVendorIdForLegalEntityCommandValidator _sut;
        
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sut = new AddEmployerVendorIdForLegalEntityCommandValidator();
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_HashedLegalEntityId_has_a_default_value()
        {
            //Arrange
            var command = new AddEmployerVendorIdForLegalEntityCommand(default, _fixture.Create<string>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [TestCase(null)]
        [TestCase("00000000")]
        public async Task Then_the_command_is_invalid_when_the_EmployerVendorId_has_an_empty_value(string value)
        {
            //Arrange
            var command = new AddEmployerVendorIdForLegalEntityCommand(_fixture.Create<string>(), value);

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }
    }
}
