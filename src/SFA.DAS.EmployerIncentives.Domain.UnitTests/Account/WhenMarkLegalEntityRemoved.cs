using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.AccountTests
{
    public class WhenMarkLegalEntityRemoved
    {
        private Account _sut;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sut = Account.New(_fixture.Create<long>());
            _sut.AddLegalEntity(_fixture.Create<long>(), _fixture.Create<LegalEntity>());
            _sut.AddLegalEntity(_fixture.Create<long>(), _fixture.Create<LegalEntity>());
        }

        [Test]
        public void Then_the_legalEntity_is_marked_as_deleted()
        {
            // Arrange
            var accountLegalEntityId = _fixture.Create<long>();
            var legalEntity = _fixture.Create<LegalEntity>();
            _sut.AddLegalEntity(accountLegalEntityId, legalEntity);
            legalEntity.GetModel().HasBeenDeleted = false;

            _sut.LegalEntities.Count.Should().Be(3);
            _sut.LegalEntities.SingleOrDefault(i => i.GetModel().AccountLegalEntityId == accountLegalEntityId).Should().NotBeNull();

            // Act
            _sut.MarkLegalEntityRemoved(legalEntity);

            // Assert
            _sut.LegalEntities.Count.Should().Be(3);
            Assert.AreEqual(legalEntity.GetModel().HasBeenDeleted, true);
        }

        [Test]
        public void Then_no_legalEntities_are_unchanged_if_it_does_not_exist()
        {
            // Arrange
            var legalEntityModel = _fixture.Create<LegalEntityModel>();
            var legalEntity = LegalEntity.Create(legalEntityModel);
            legalEntity.GetModel().HasBeenDeleted = false;

            _sut.LegalEntities.Count.Should().Be(2);
            _sut.LegalEntities.SingleOrDefault(i => i.GetModel().AccountLegalEntityId == legalEntityModel.AccountLegalEntityId).Should().BeNull();

            // Act
            _sut.MarkLegalEntityRemoved(legalEntity);

            // Assert
            _sut.LegalEntities.Count.Should().Be(2);
            _sut.LegalEntities.SingleOrDefault(i => i.GetModel().AccountLegalEntityId == legalEntityModel.AccountLegalEntityId).Should().BeNull();
            Assert.AreEqual(legalEntity.GetModel().HasBeenDeleted, false);
        }
    }
}
