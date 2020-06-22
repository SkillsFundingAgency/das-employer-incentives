using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.Data;
using SFA.DAS.EmployerIncentives.Domain.Entities;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using System.Collections.ObjectModel;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.AccountTests
{
    public class WhenGetModelCalled
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
            var initialisedModel = _fixture
                .Build<AccountModel>()
                .With(f => f.LegalEntityModels, new Collection<ILegalEntityModel> { _fixture.Create<LegalEntityModel>(), _fixture.Create<LegalEntityModel>()})
                .Create();

            var sut = Account.Create(initialisedModel);

            // Act
            var model = sut.GetModel();

            // Assert
            model.Should().Be(initialisedModel);
        }
    }
}
