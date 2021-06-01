using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.RemoveLegalEntity;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Account = SFA.DAS.EmployerIncentives.Domain.Accounts.Account;

namespace SFA.DAS.EmployerIncentives.Application.UnitTests.RemoveLegalEntity.Handlers
{
    [TestFixture]
    public class WhenHandlingRemoveLegalEntityCommand
    {
        private RemoveLegalEntityCommandHandler _sut;
        private Mock<IAccountDomainRepository> _accountDomainRepository;
        
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _accountDomainRepository = new Mock<IAccountDomainRepository>();
          
            _sut = new RemoveLegalEntityCommandHandler(_accountDomainRepository.Object);
        }

        [Test]
        public async Task Then_the_account_changes_are_persisted_to_the_domain_repository()
        {
            //Arrange
            var command = _fixture.Create<RemoveLegalEntityCommand>();
            var legalEntityModel = _fixture.Create<LegalEntityModel>();
            legalEntityModel.Id = command.AccountId;
            legalEntityModel.AccountLegalEntityId = command.AccountLegalEntityId;

            _accountDomainRepository
                .Setup(m => m.Find(command.AccountId))
                .ReturnsAsync(Account.Create(
                    new AccountModel
                    {
                        Id = command.AccountId,
                        LegalEntityModels = new Collection<LegalEntityModel>() {
                            _fixture.Create<LegalEntityModel>(),
                            legalEntityModel,
                            _fixture.Create<LegalEntityModel>(),
                        }
                    }));

            //Act
            await _sut.Handle(command);

            //Assert
            _accountDomainRepository.Verify(m => m.Save(It.Is<Account>(i => i.Id == command.AccountId && i.LegalEntities.Count == 2)), Times.Once);
        }

        [Test]
        public async Task Then_the_account_changes_are_not_persisted_to_the_domain_repository_if_it_does_not_exist()
        {
            //Arrange
            var command = _fixture.Create<RemoveLegalEntityCommand>();
            _accountDomainRepository
                .Setup(m => m.Find(command.AccountId))
                .ReturnsAsync(null as Account);

            //Act
            await _sut.Handle(command);

            //Assert
            _accountDomainRepository.Verify(m => m.Save(It.Is<Account>(i => i.Id == command.AccountId)), Times.Never);
        }

        [Test]
        public async Task Then_the_account_changes_are_not_persisted_to_the_domain_repository_if_the_account_exists_but_the_legalEntity_does_not_exist()
        {
            //Arrange
            var command = _fixture.Create<RemoveLegalEntityCommand>();
            _accountDomainRepository
                .Setup(m => m.Find(command.AccountId))
                .ReturnsAsync(Account.Create(new AccountModel { Id = 1, LegalEntityModels = new Collection<LegalEntityModel>() { _fixture.Create<LegalEntityModel>() } }));

            //Act
            await _sut.Handle(command);

            //Assert
            _accountDomainRepository.Verify(m => m.Save(It.Is<Account>(i => i.Id == command.AccountId)), Times.Never);
        }

    }
}
