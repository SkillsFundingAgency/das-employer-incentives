using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;
using System.Collections.ObjectModel;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.AccountTests
{
    public class GetLegalEntityCalled
    {
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void Then_the_model_is_returned()
        {
            // Arrange     
            var legalEntityModel = _fixture.Create<LegalEntityModel>();
            var accountModel = _fixture
                .Build<AccountModel>()
                .With(f => f.LegalEntityModels, 
                new Collection<LegalEntityModel> { 
                    _fixture.Create<LegalEntityModel>(),
                    legalEntityModel,
                    _fixture.Create<LegalEntityModel>()})
                .Create();

            var sut = Account.Create(accountModel);

            // Act
            var model = sut.GetLegalEntity(legalEntityModel.AccountLegalEntityId);

            // Assert
            model.Should().Be(LegalEntity.Create(legalEntityModel));
        }

        [Test]
        public void Then_the_model_returned_is_null_if_the_legalEntity_does_not_exist()
        {
            // Arrange     
            var legalEntityModel = _fixture.Create<LegalEntityModel>();
            var accountModel = _fixture
                .Build<AccountModel>()
                .With(f => f.LegalEntityModels,
                new Collection<LegalEntityModel> {
                    _fixture.Create<LegalEntityModel>(),
                    _fixture.Create<LegalEntityModel>(),
                    _fixture.Create<LegalEntityModel>()})
                .Create();

            var sut = Account.Create(accountModel);

            // Act
            var model = sut.GetLegalEntity(legalEntityModel.AccountLegalEntityId);

            // Assert
            model.Should().BeNull();
        }
    }
}
