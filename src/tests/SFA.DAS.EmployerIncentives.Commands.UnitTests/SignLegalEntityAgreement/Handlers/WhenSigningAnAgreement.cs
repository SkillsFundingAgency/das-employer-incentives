using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.SignLegalEntityAgreement;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.SignLegalEntityAgreement.Handlers
{
    public class WhenSigningAnAgreement
    {
        private SignLegalEntityAgreementCommandHandler _sut;
        private Mock<IAccountDomainRepository> _mockDomainRespository;
        
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockDomainRespository = new Mock<IAccountDomainRepository>();
            
            _sut = new SignLegalEntityAgreementCommandHandler(_mockDomainRespository.Object);
        }

        [Test]
        public async Task Then_the_agreement_status_of_the_legal_entity_is_updated()
        {
            //Arrange
            var command = _fixture.Create<SignLegalEntityAgreementCommand>();
            var expectedAccount = Account.Create(new AccountModel { Id = 1, LegalEntityModels = new Collection<LegalEntityModel>() });
            _mockDomainRespository
                .Setup(m => m.Find(command.AccountId))
                .ReturnsAsync(expectedAccount);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockDomainRespository.Verify(m => m.Save(expectedAccount), Times.Once);
        }
    }
}
