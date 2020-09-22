using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.UpdateVrfCaseDetailsForLegalEntity;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.UpdateVrfCaseDetailsForLegalEntity.Handlers
{
    public class WhenHandlingUpdateVrfCaseDetailsForLegalEntityCommand
    {
        private UpdateVrfCaseDetailsForLegalEntityCommandHandler _sut;
        private Mock<IAccountDomainRepository> _mockDomainRepository;

        private Fixture _fixture;

        private const long LegalEntityToBeUpdatedId = -123;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize(new IncentiveApplicationCustomization());

            _mockDomainRepository = new Mock<IAccountDomainRepository>();

            _sut = new UpdateVrfCaseDetailsForLegalEntityCommandHandler(_mockDomainRepository.Object);
        }

        [Test]
        public async Task Then_VRF_details_for_given_legal_entity_are_updated()
        {
            // Arrange
            var command = new UpdateVrfCaseDetailsForLegalEntityCommand(LegalEntityToBeUpdatedId, _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());

            var accounts = _fixture.CreateMany<Account>(3).ToList();
            var legalEntityToBeUpdated = LegalEntity.New(LegalEntityToBeUpdatedId, _fixture.Create<string>());

            accounts[0].AddLegalEntity(1, legalEntityToBeUpdated);
            accounts[0].AddLegalEntity(2, _fixture.Create<LegalEntity>());
            accounts[0].AddLegalEntity(3, _fixture.Create<LegalEntity>());

            accounts[1].AddLegalEntity(4, _fixture.Create<LegalEntity>());
            accounts[1].AddLegalEntity(5, legalEntityToBeUpdated);

            _mockDomainRepository.Setup(x => x.GetByLegalEntityId(command.LegalEntityId)).ReturnsAsync(accounts);

            // Act
            await _sut.Handle(command);

            // Assert
            var updatedLegalEntities = accounts.SelectMany(x => x.LegalEntities.Where(e => e.Id == command.LegalEntityId)).ToList();
            foreach (var legalEntity in updatedLegalEntities)
            {
                legalEntity.VrfCaseId.Should().Be(command.CaseId);
                legalEntity.VrfVendorId.Should().Be(command.VendorId);
                legalEntity.VrfCaseStatus.Should().Be(command.Status);
            }

            _mockDomainRepository.Verify(m => m.Save(accounts[0]), Times.Once);
            _mockDomainRepository.Verify(m => m.Save(accounts[1]), Times.Once);
            _mockDomainRepository.VerifyAll();
        }
    }
}
