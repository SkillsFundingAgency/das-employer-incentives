using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Commands.UpsertLegalEntity;
using SFA.DAS.HashingService;

namespace SFA.DAS.EmployerIncentives.Application.UnitTests.UpsertLegalEntity.Handlers
{
    public class WhenHandlingUpsertLegalEntityCommand
    {
        private UpsertLegalEntityCommandHandler _sut;
        private Mock<IAccountDomainRepository> _mockDomainRespository;
        private Mock<IHashingService> _mockHashingService;
        
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockDomainRespository = new Mock<IAccountDomainRepository>();
            _mockHashingService = new Mock<IHashingService>();
            
            _sut = new UpsertLegalEntityCommandHandler(_mockDomainRespository.Object, _mockHashingService.Object);
        }

        [Test]
        public async Task Then_the_a_new_account_is_persisted_to_the_domain_repository()
        {
            //Arrange
            var command = _fixture.Create<UpsertLegalEntityCommand>();
            var expectedHash = _fixture.Create<string>();
            _mockHashingService.Setup(x => x.HashValue(command.LegalEntityId)).Returns(expectedHash);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockDomainRespository.Verify(m => m.Save(It.Is<Account>(i => i.Id == command.AccountId && i.LegalEntities.Single(x => x.Id == command.LegalEntityId).HashedLegalEntityId == expectedHash)), Times.Once);
        }

        [Test]
        public async Task Then_an_existing_legal_entity_without_a_hashed_id_is_persisted_to_the_domain_repository()
        {
            //Arrange
            var command = _fixture.Create<UpsertLegalEntityCommand>();
            var expectedHash = _fixture.Create<string>();
            _mockHashingService.Setup(x => x.HashValue(command.LegalEntityId)).Returns(expectedHash);

            _mockDomainRespository
                .Setup(m => m.Find(command.AccountId))
                .ReturnsAsync(Account.Create(new AccountModel { Id = command.AccountId, LegalEntityModels = new Collection<LegalEntityModel> { new LegalEntityModel { Id = command.LegalEntityId, AccountLegalEntityId = command.AccountLegalEntityId, HashedLegalEntityId = null } } }));

            //Act
            await _sut.Handle(command);

            //Assert
            _mockDomainRespository.Verify(m => m.Save(It.Is<Account>(i => i.Id == command.AccountId && i.LegalEntities.Single(x => x.Id == command.LegalEntityId).HashedLegalEntityId == expectedHash)), Times.Once);
        }

        [Test]
        public async Task Then_the_legal_entity_name_is_persisted_to_the_domain_repository()
        {
            //Arrange
            var command = _fixture.Create<UpsertLegalEntityCommand>();
            
            _mockDomainRespository
                .Setup(m => m.Find(command.AccountId))
                .ReturnsAsync(Account.Create(new AccountModel { Id = command.AccountId, LegalEntityModels = new Collection<LegalEntityModel> { new LegalEntityModel { Id = command.LegalEntityId, AccountLegalEntityId = command.AccountLegalEntityId, HashedLegalEntityId = _fixture.Create<string>(), Name = _fixture.Create<string>() } } }));

            //Act
            await _sut.Handle(command);

            //Assert
            _mockDomainRespository.Verify(m => m.Save(It.Is<Account>(i => i.Id == command.AccountId && i.LegalEntities.Single(x => x.Id == command.LegalEntityId).Name == command.Name)), Times.Once);
        }
    }
}
