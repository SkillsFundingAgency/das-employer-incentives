using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Application.UnitTests.Persistence
{
    public class WhenSavingAnAccountAggregate
    {
        private AccountDomainRepository _sut;
        private Mock<IAccountDataRepository> _mockAccountDataRepository;
        private Mock<IDomainEventDispatcher> _mockDomainEventDispatcher;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockAccountDataRepository = new Mock<IAccountDataRepository>();
            _mockDomainEventDispatcher = new Mock<IDomainEventDispatcher>();

            _sut = new AccountDomainRepository(_mockAccountDataRepository.Object, _mockDomainEventDispatcher.Object);
        }

        [Test]
        public async Task Then_a_new_entity_is_persisted_by_the_data_layer()
        {
            //Arrange
            var entity = _fixture.Create<Account>();

            //Act
            await _sut.Save(entity);

            //Assert
            _mockAccountDataRepository.Verify(m => m.Add(entity.GetModel()), Times.Once);
        }
    }
}
