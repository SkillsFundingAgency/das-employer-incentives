using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
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
        private Mock<IAccountDomainRepository> _mockDomainRepository;
        private Mock<IOptions<ApplicationSettings>> _mockOptions;

        private Fixture _fixture;

        private const int ExpectedAgreementVersion = 2;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockDomainRepository = new Mock<IAccountDomainRepository>();
            _mockOptions = new Mock<IOptions<ApplicationSettings>>();
            _mockOptions.Setup(x => x.Value.MinimumAgreementVersion).Returns(ExpectedAgreementVersion);

            _sut = new SignLegalEntityAgreementCommandHandler(_mockDomainRepository.Object, _mockOptions.Object);
        }

        [Test]
        public async Task Then_the_agreement_status_of_the_legal_entity_is_updated()
        {
            //Arrange
            var command = new SignLegalEntityAgreementCommand(
                _fixture.Create<long>(), 
                _fixture.Create<long>(),
                ExpectedAgreementVersion, 
                _fixture.Create<string>(),
                _fixture.Create<long>()
                );
            var legalEntityModel = new LegalEntityModel { Id = command.AccountLegalEntityId, AccountLegalEntityId = command.AccountLegalEntityId };
            var expectedAccount = Account.Create(new AccountModel { Id = 1, LegalEntityModels = new Collection<LegalEntityModel> { legalEntityModel } });

            _mockDomainRepository
                .Setup(m => m.Find(command.AccountId))
                .ReturnsAsync(expectedAccount);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockDomainRepository.Verify(m => m.Save(expectedAccount), Times.Once);
        }

        [Test]
        public async Task Then_an_account_is_created_if_doesnt_exist()
        {
            //Arrange
            var command = new SignLegalEntityAgreementCommand(
                _fixture.Create<long>(), 
                _fixture.Create<long>(),
                ExpectedAgreementVersion, 
                _fixture.Create<string>(),
                _fixture.Create<long>()
                );

           _mockDomainRepository
                .Setup(m => m.Find(command.AccountId))
                .ReturnsAsync((Account) null);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockDomainRepository.Verify(m => m.Save(It.Is<Account>(
                a => a.Id == command.AccountId
                     && a.LegalEntities.First().GetModel().AccountLegalEntityId == command.AccountLegalEntityId
                     && a.LegalEntities.First().GetModel().Name == command.LegalEntityName
                     && a.LegalEntities.First().GetModel().SignedAgreementVersion == ExpectedAgreementVersion
                )), Times.Once);
        }
    }
}
