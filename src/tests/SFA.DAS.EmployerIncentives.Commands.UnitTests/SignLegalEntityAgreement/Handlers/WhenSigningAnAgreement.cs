using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.SignLegalEntityAgreement;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.SignLegalEntityAgreement.Handlers
{
    public class WhenSigningAnAgreement
    {
        private SignLegalEntityAgreementCommandHandler _sut;
        private Mock<IAccountDomainRepository> _mockDomainRespository;
        private Mock<IOptions<ApplicationSettings>> _mockOptions;

        private Fixture _fixture;

        private const int ExpectedMinimumVersion = 2;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockDomainRespository = new Mock<IAccountDomainRepository>();
            _mockOptions = new Mock<IOptions<ApplicationSettings>>();
            _mockOptions.Setup(x => x.Value.MinimumAgreementVersion).Returns(ExpectedMinimumVersion);

            _sut = new SignLegalEntityAgreementCommandHandler(_mockDomainRespository.Object, _mockOptions.Object);
        }

        [Test]
        public async Task Then_the_agreement_status_of_the_legal_entity_is_updated()
        {
            //Arrange
            var command = new SignLegalEntityAgreementCommand(_fixture.Create<long>(), _fixture.Create<long>(), ExpectedMinimumVersion);
            var legalEntityModel = new LegalEntityModel { Id = command.AccountLegalEntityId, AccountLegalEntityId = command.AccountLegalEntityId, HasSignedAgreementTerms = false };
            var expectedAccount = Account.Create(new AccountModel { Id = 1, LegalEntityModels = new Collection<LegalEntityModel> { legalEntityModel } });
            _mockDomainRespository
                .Setup(m => m.Find(command.AccountId))
                .ReturnsAsync(expectedAccount);

            //Act
            await _sut.Handle(command);

            //Assert
            legalEntityModel.HasSignedAgreementTerms.Should().BeTrue();
            _mockDomainRespository.Verify(m => m.Save(expectedAccount), Times.Once);
        }
    }
}
