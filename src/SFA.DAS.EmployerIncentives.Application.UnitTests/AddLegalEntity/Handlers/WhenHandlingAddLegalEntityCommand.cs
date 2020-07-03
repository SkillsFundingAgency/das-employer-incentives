using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.AddLegalEntity;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Application.UnitTests.AddLegalEntity.Handlers
{
    public class WhenHandlingAddLegalEntityCommand
    {
        private AddLegalEntityCommandHandler _sut;
        private Mock<IAccountDomainRepository> _mockDomainRespository;
        
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockDomainRespository = new Mock<IAccountDomainRepository>();
            
            _sut = new AddLegalEntityCommandHandler(_mockDomainRespository.Object);
        }

        [Test]
        public async Task Then_the_a_new_account_is_persisted_to_the_domain_repository()
        {
            //Arrange
            var command = _fixture.Create<AddLegalEntityCommand>();

            //Act
            await _sut.Handle(command);

            //Assert
            _mockDomainRespository.Verify(m => m.Save(It.Is<Account>(i => i.Id == command.AccountId)), Times.Once);
        }

        [Test]
        public async Task Then_the_the_account_is_not_persisted_to_the_domain_repository_if_it_already_exists()
        {
            //Arrange
            var command = _fixture.Create<AddLegalEntityCommand>();
            _mockDomainRespository
                .Setup(m => m.Find(command.AccountId))
                .ReturnsAsync(Account.Create(new AccountModel { Id = 1, LegalEntityModels = new Collection<LegalEntityModel>() }));

            //Act
            await _sut.Handle(command);

            //Assert
            _mockDomainRespository.Verify(m => m.Save(It.Is<Account>(i => i.Id == command.AccountId)), Times.Never);
        }
    }
}
