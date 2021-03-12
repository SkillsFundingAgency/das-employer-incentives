using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.RefreshLegalEntities;
using SFA.DAS.EmployerIncentives.Commands.Services.AccountApi;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Application.UnitTests.RefreshLegalEntities.Validators
{
    [TestFixture]
    public class WhenValidatingRefreshLegalEntityCommand
    {
        private RefreshLegalEntityCommandValidator _sut;

        private Fixture _fixture;

        private List<AccountLegalEntity> _accountLegalEntities;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _accountLegalEntities = _fixture.CreateMany<AccountLegalEntity>(10).ToList();

            _sut = new RefreshLegalEntityCommandValidator();
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_pageNumber_has_a_default_value()
        {
            //Arrange
            var command = new RefreshLegalEntitiesCommand(_accountLegalEntities, default, _fixture.Create<int>(), _fixture.Create<int>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }
        [Test]
        public async Task Then_the_command_is_invalid_when_the_pageSize_has_a_default_value()
        {
            //Arrange
            var command = new RefreshLegalEntitiesCommand(_accountLegalEntities, _fixture.Create<int>(), default, _fixture.Create<int>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_invalid_when_the_account_legal_entities_are_not_set()
        {
            //Arrange
            var command = new RefreshLegalEntitiesCommand(default, _fixture.Create<int>(), _fixture.Create<int>(), _fixture.Create<int>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_command_is_valid()
        {
            //Arrange
            var command = new RefreshLegalEntitiesCommand(_accountLegalEntities, _fixture.Create<int>(), _fixture.Create<int>(), _fixture.Create<int>());

            //Act
            var result = await _sut.Validate(command);

            //Assert
            result.ValidationDictionary.Count.Should().Be(0);
        }
    }
}
