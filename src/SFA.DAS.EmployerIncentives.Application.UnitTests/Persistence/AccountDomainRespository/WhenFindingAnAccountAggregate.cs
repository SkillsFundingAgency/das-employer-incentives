using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Application.Persistence;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Domain.Data;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Application.UnitTests.Persistence
{
    public class WhenFindingAnAccountAggregate
    {
        private AccountDomainRepository _sut;
        private Mock<IAccountDataRepository> _mockAccountDataRepository;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockAccountDataRepository = new Mock<IAccountDataRepository>();

            _sut = new AccountDomainRepository(_mockAccountDataRepository.Object);
        }

        [Test]
        public async Task Then_the_account_data_is_retrieved_from_the_data_layer()
        {
            //Arrange
            var accountId = _fixture.Create<long>();

            //Act
            await _sut.Find(accountId);

            //Assert
            _mockAccountDataRepository.Verify(m => m.Find(accountId), Times.Once);
        }

        [Test]
        public async Task Then_the_account_is_returned_when_it_exists()
        {
            //Arrange
            var accountModel = _fixture.Build<AccountModel>().With(f => f.LegalEntityModel, _fixture.Create<LegalEntityModel>()).Create();

            _mockAccountDataRepository
                .Setup(m => m.Find(accountModel.Id))
                .ReturnsAsync(accountModel);

            //Act
            var account = await _sut.Find(accountModel.Id);

            //Assert
            account.Should().NotBeNull();
            account.Id.Should().Be(accountModel.Id);
            account.AccountLegalEntityId.Should().Be(accountModel.AccountLegalEntityId);
            account.LegalEntity.Id.Should().Be(accountModel.LegalEntityModel.Id);
            account.LegalEntity.Name.Should().Be(accountModel.LegalEntityModel.Name);
        }

        [Test]
        public async Task Then_an_account_is_not_returned_when_it_does_not_exist()
        {
            //Arrange
            var accountId = _fixture.Create<long>();

            _mockAccountDataRepository
                .Setup(m => m.Find(accountId))
                .ReturnsAsync(null as AccountModel);

            //Act
            var account = await _sut.Find(accountId);

            //Assert
            account.Should().BeNull();
        }
    }
}
