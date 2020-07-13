using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.RefreshLegalEntities;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Application.UnitTests.RefreshLegalEntities.Validators
{
    public class WhenValidatingRefreshLegalEntityCommand
    {
        private RefreshLegalEntityCommandValidator _sut;
        
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sut = new RefreshLegalEntityCommandValidator();
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_pageNumber_has_a_default_value()
        {
            //Arrange
            var command = new RefreshLegalEntitiesCommand(default, _fixture.Create<int>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }
        [Test]
        public async Task Then_the_command_is_invalid_when_the_pageSize_has_a_default_value()
        {
            //Arrange
            var command = new RefreshLegalEntitiesCommand(_fixture.Create<int>(), default);

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }
       
        [Test]
        public async Task Then_the_command_is_valid()
        {
            //Arrange
            var command = new RefreshLegalEntitiesCommand();

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(0);
        }
    }
}
