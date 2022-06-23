using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.Persistence.AccountDomainRepository
{
    [TestFixture]
    public class WhenFindingAccountByVendorId
    {
        private Commands.Persistence.AccountDomainRepository _sut;
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
            //Arrange
            var vendorId = _fixture.Create<string>();
            _mockAccountDataRepository.Setup(x => x.FindByVendorId(vendorId))
                .ReturnsAsync(new List<AccountModel> {_fixture.Create<AccountModel>()});

            //Act
            await _sut.FindByVendorId(vendorId);

            //Assert
            _mockAccountDataRepository.Verify(m => m.FindByVendorId(vendorId), Times.Once);
        }
    }
}
