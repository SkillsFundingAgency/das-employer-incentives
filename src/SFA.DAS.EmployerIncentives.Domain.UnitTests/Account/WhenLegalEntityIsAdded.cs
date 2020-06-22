using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.Data;
using SFA.DAS.EmployerIncentives.Domain.Entities;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;
using System;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.AccountTests
{
    public class WhenLegalEntityIsAdded
    {
        private Domain.Entities.Account _sut;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sut = Domain.Entities.Account.New(_fixture.Create<long>());
        }

        [Test]
        public void Then_the_legalEntity_is_added()
        {
            // Arrange
            var legalEntity = LegalEntity.Create(_fixture.Create<LegalEntityModel>());

            // Act
            _sut.AddLegalEntity(_fixture.Create<long>(), legalEntity);

            // Assert
            _sut.LegalEntities.Single().Id.Should().Be(legalEntity.Id);
        }

        [Test]
        public void Then_the_accountLegalEntityId_is_set()
        {
            // Arrange
            var accountLegalEntityId = _fixture.Create<long>();

            // Act
            _sut.AddLegalEntity(accountLegalEntityId, LegalEntity.Create(_fixture.Create<LegalEntityModel>()));

            // Assert
            _sut.GetModel().LegalEntityModels.Single().AccountLegalEntityId.Should().Be(accountLegalEntityId);
        }

        [Test]
        public void Then_an_InvalidMethodCallException_is_raised_if_the_same_legalEntity_has_already_been_added()
        {
            // Arrange
            var acountLegalEntity = _fixture.Create<long>();
            _sut.AddLegalEntity(acountLegalEntity, LegalEntity.Create( _fixture.Create<LegalEntityModel>()));

            // Act
            Action action = () => _sut.AddLegalEntity(acountLegalEntity, LegalEntity.Create(_fixture.Create<LegalEntityModel>()));

            //Assert
            action.Should().Throw<InvalidMethodCallException>().WithMessage("Legal entity has already been added");
        }

        [Test]
        public void Then_the_legalEntity_is_added_if_it_is_different_to_a_previously_added_one()
        {
            // Arrange
            var firstAccountLegalEntityId = _fixture.Create<long>();
            var firstLegalEntity = LegalEntity.Create(_fixture.Build<LegalEntityModel>().With(f => f.AccountLegalEntityId, firstAccountLegalEntityId).Create());
            
            _sut.AddLegalEntity(firstAccountLegalEntityId, firstLegalEntity);

            var secondAccountLegalEntityId = firstAccountLegalEntityId + 1;
            var secondLegalEntity = LegalEntity.Create(_fixture.Build<LegalEntityModel>().With(f => f.Id, firstLegalEntity.Id + 1).With(f => f.AccountLegalEntityId, secondAccountLegalEntityId).Create());
            
            // Act
            _sut.AddLegalEntity(secondAccountLegalEntityId, secondLegalEntity);

            // Assert
            _sut.LegalEntities.Should().Contain(new [] { firstLegalEntity , secondLegalEntity});
        }
    }
}
