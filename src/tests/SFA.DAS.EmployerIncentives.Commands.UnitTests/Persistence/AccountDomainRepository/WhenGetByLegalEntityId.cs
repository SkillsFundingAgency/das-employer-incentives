using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Events;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.Persistence.AccountDomainRepository
{
    public class WhenGetByLegalEntityId
    {
        private IAccountDomainRepository _sut;
        private Mock<IAccountDataRepository> _mockAccountDataRepository;
        private Mock<IDomainEventDispatcher> _mockDomainEventDispatcher;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockAccountDataRepository = new Mock<IAccountDataRepository>();
            _mockDomainEventDispatcher = new Mock<IDomainEventDispatcher>();
            _sut = new Commands.Persistence.AccountDomainRepository(_mockAccountDataRepository.Object, _mockDomainEventDispatcher.Object);
        }

        [Test]
        public async Task Then_the_account_data_is_retrieved_from_the_data_layer()
        {
            // Arrange
            var legalEntityId = _fixture.Create<long>();

            // Act
            await _sut.GetByLegalEntityId(legalEntityId);

            // Assert
            _mockAccountDataRepository.Verify(m => m.GetByLegalEntityId(legalEntityId), Times.Once);
        }

        [Test]
        public async Task Then_the_account_is_returned_when_it_exists()
        {
            // Arrange
            var accountModels = _fixture.CreateMany<AccountModel>(3).ToList();
            var legalEntity = _fixture.Create<LegalEntityModel>();
            legalEntity.Id = -1;

            accountModels[0].LegalEntityModels.Add(legalEntity);
            accountModels[1].LegalEntityModels.Add(legalEntity);
            accountModels[2].LegalEntityModels = _fixture.CreateMany<LegalEntityModel>().ToList();

            _mockAccountDataRepository
                .Setup(m => m.GetByLegalEntityId(legalEntity.Id))
                .ReturnsAsync(accountModels);

            // Act
            var accounts = await _sut.GetByLegalEntityId(legalEntity.Id);

            // Assert
            var account = accounts.First();
            account.Should().BeEquivalentTo(accountModels.First(), opt => opt.ExcludingMissingMembers());
            account.LegalEntities.Should().BeEquivalentTo(accountModels.First().LegalEntityModels,
                opt => opt.ExcludingMissingMembers());
        }
    }
}
