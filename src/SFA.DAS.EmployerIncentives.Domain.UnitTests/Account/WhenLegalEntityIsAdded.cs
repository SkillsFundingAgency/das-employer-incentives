using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.Data;
using SFA.DAS.EmployerIncentives.Domain.Entities;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;
using System;

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
        public void Then_the_legalEntity_is_set()
        {
            // Arrange
            var legalEntity = LegalEntity.Create(_fixture.Create<LegalEntityModel>());

            // Act
            _sut.AddLegalEntity(_fixture.Create<long>(), legalEntity);

            // Assert
            _sut.LegalEntity.Should().Be(legalEntity);
        }

        [Test]
        public void Then_the_accountLegalEntityId_is_set()
        {
            // Arrange
            var accountLegalEntityId = _fixture.Create<long>();

            // Act
            _sut.AddLegalEntity(accountLegalEntityId, LegalEntity.Create(_fixture.Create<LegalEntityModel>()));

            // Assert
            _sut.AccountLegalEntityId.Should().Be(accountLegalEntityId);
        }

        [Test]
        public void Then_an_InvalidMethodCallException_is_raised_if_the_legalEntity_has_already_been_set()
        {
            // Arrange
            _sut.AddLegalEntity(_fixture.Create<long>(), LegalEntity.Create( _fixture.Create<LegalEntityModel>()));

            // Act
            Action action = () => _sut.AddLegalEntity(_fixture.Create<long>(), LegalEntity.Create(_fixture.Create<LegalEntityModel>()));

            //Assert
            action.Should().Throw<InvalidMethodCallException>().WithMessage("Legal entity has already been set up");
        }
    }
}
