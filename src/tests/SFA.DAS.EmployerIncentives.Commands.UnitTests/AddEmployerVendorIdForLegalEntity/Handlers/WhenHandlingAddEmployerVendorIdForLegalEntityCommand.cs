using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.AddEmployerVendorIdForLegalEntity;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.AddEmployerVendorIdForLegalEntity.Handlers
{
    public class WhenHandlingAddEmployerVendorIdForLegalEntityCommand
    {
        private AddEmployerVendorIdForLegalEntityCommandHandler _sut;
        private Mock<IAccountDomainRepository> _mockDomainRepository;
        private Fixture _fixture;
        private const string LegalEntityToBeUpdatedId = "XYZ123";

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize(new IncentiveApplicationCustomization());

            _mockDomainRepository = new Mock<IAccountDomainRepository>();

            _sut = new AddEmployerVendorIdForLegalEntityCommandHandler(_mockDomainRepository.Object);
        }

        [Test]
        public async Task Then_VRF_EmployerVendorId_is_added_to_legal_entities_which_dont_have_a_vendor_id_assigned()
        {
            // Arrange
            var command = new AddEmployerVendorIdForLegalEntityCommand(LegalEntityToBeUpdatedId, _fixture.Create<string>());

            var accounts = _fixture.CreateMany<Account>(3).ToList();
            var legalEntityToBeUpdated = LegalEntity.New(123, _fixture.Create<string>(), LegalEntityToBeUpdatedId);

            accounts[0].AddLegalEntity(1, legalEntityToBeUpdated);
            accounts[0].AddLegalEntity(2, _fixture.Create<LegalEntity>());
            accounts[0].AddLegalEntity(3, _fixture.Create<LegalEntity>());

            accounts[1].AddLegalEntity(4, _fixture.Create<LegalEntity>());
            accounts[1].AddLegalEntity(5, legalEntityToBeUpdated);

            _mockDomainRepository.Setup(x => x.GetByHashedLegalEntityId(command.HashedLegalEntityId)).ReturnsAsync(accounts);

            // Act
            await _sut.Handle(command);

            // Assert
            var updatedLegalEntities = accounts.SelectMany(x => x.LegalEntities.Where(e => e.HashedLegalEntityId == command.HashedLegalEntityId)).ToList();
            updatedLegalEntities.Should().NotBeEmpty();
            foreach (var legalEntity in updatedLegalEntities)
            {
                legalEntity.VrfVendorId.Should().Be(command.EmployerVendorId);
            }

            _mockDomainRepository.Verify(m => m.Save(accounts[0]), Times.Once);
            _mockDomainRepository.Verify(m => m.Save(accounts[1]), Times.Once);
            _mockDomainRepository.VerifyAll();
        }

        [Test]
        public async Task Then_VRF_EmployerVendorId_is_not_added_to_legal_entities_which_do_have_a_vendor_id_assigned()
        {
            // Arrange
            var command = new AddEmployerVendorIdForLegalEntityCommand(LegalEntityToBeUpdatedId, _fixture.Create<string>());

            var accounts = _fixture.CreateMany<Account>(3).ToList();
            var legalEntityToBeUpdated = LegalEntity.New(123, _fixture.Create<string>(), LegalEntityToBeUpdatedId);
            legalEntityToBeUpdated.AddEmployerVendorId(_fixture.Create<string>());

            accounts[0].AddLegalEntity(1, legalEntityToBeUpdated);
            accounts[0].AddLegalEntity(2, _fixture.Create<LegalEntity>());
            accounts[0].AddLegalEntity(3, _fixture.Create<LegalEntity>());

            accounts[1].AddLegalEntity(4, _fixture.Create<LegalEntity>());
            accounts[1].AddLegalEntity(5, legalEntityToBeUpdated);

            _mockDomainRepository.Setup(x => x.GetByHashedLegalEntityId(command.HashedLegalEntityId)).ReturnsAsync(accounts);

            // Act
            await _sut.Handle(command);

            // Assert
            var updatedLegalEntities = accounts.SelectMany(x => x.LegalEntities.Where(e => e.HashedLegalEntityId == command.HashedLegalEntityId)).ToList();
            updatedLegalEntities.Should().NotBeEmpty();
            foreach (var legalEntity in updatedLegalEntities)
            {
                legalEntity.VrfVendorId.Should().NotBe(command.EmployerVendorId);
            }

            _mockDomainRepository.Verify(m => m.Save(accounts[0]), Times.Once);
            _mockDomainRepository.Verify(m => m.Save(accounts[1]), Times.Once);
            _mockDomainRepository.VerifyAll();
        }
    }
}
