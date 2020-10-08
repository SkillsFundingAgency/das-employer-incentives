using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.UpdateVrfCaseStatusForLegalEntity;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Events;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.UpdateVrfCaseStatusForLegalEntity.Handlers
{
    public class WhenHandlingUpdateVrfCaseDetailsForLegalEntityCommand
    {
        private UpdateVendorRegistrationCaseStatusCommandHandler _sut;
        private Mock<IAccountDomainRepository> _mockDomainRepository;
        private Fixture _fixture;
        private const string LegalEntityToBeUpdatedId = "XYZ123";

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize(new IncentiveApplicationCustomization());

            _mockDomainRepository = new Mock<IAccountDomainRepository>();

            _sut = new UpdateVendorRegistrationCaseStatusCommandHandler(_mockDomainRepository.Object);
        }

        [Test]
        public async Task Then_VRF_details_for_given_legal_entity_are_updated()
        {
            // Arrange
            var command = new UpdateVendorRegistrationCaseStatusCommand(LegalEntityToBeUpdatedId, _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(),
                _fixture.Create<DateTime>());

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
                legalEntity.VrfCaseId.Should().Be(command.CaseId);
                legalEntity.VrfVendorId.Should().Be(command.VendorId);
                legalEntity.VrfCaseStatus.Should().Be(command.Status);
                legalEntity.VrfCaseStatusLastUpdatedDateTime.Should().Be(command.CaseStatusLastUpdatedDate);
            }

            _mockDomainRepository.Verify(m => m.Save(accounts[0]), Times.Once);
            _mockDomainRepository.Verify(m => m.Save(accounts[1]), Times.Once);
            _mockDomainRepository.VerifyAll();
        }


        [Test]
        public async Task Then_VRF_details_for_given_legal_entity_are_not_updated_if_status_is_completed()
        {
            // Arrange
            var caseCompletedCommand = new UpdateVendorRegistrationCaseStatusCommand(LegalEntityToBeUpdatedId, _fixture.Create<string>(), _fixture.Create<string>(), "Case request complete",
                _fixture.Create<DateTime>());

            var accounts = _fixture.CreateMany<Account>(3).ToList();
            var legalEntityToBeUpdated = LegalEntity.New(123, _fixture.Create<string>(), LegalEntityToBeUpdatedId);

            accounts[0].AddLegalEntity(1, legalEntityToBeUpdated);
            accounts[0].AddLegalEntity(2, _fixture.Create<LegalEntity>());
            accounts[0].AddLegalEntity(3, _fixture.Create<LegalEntity>());

            accounts[1].AddLegalEntity(4, _fixture.Create<LegalEntity>());
            accounts[1].AddLegalEntity(5, legalEntityToBeUpdated);

            // Act
            _mockDomainRepository.Setup(x => x.GetByHashedLegalEntityId(caseCompletedCommand.HashedLegalEntityId)).ReturnsAsync(accounts);
            await _sut.Handle(caseCompletedCommand);

            // Assert
            var updatedLegalEntities = accounts.SelectMany(x => x.LegalEntities.Where(e => e.HashedLegalEntityId == caseCompletedCommand.HashedLegalEntityId)).ToList();
            updatedLegalEntities.Should().NotBeEmpty();
            foreach (var legalEntity in updatedLegalEntities)
            {
                legalEntity.VrfCaseId.Should().NotBe(caseCompletedCommand.CaseId);
                legalEntity.VrfVendorId.Should().NotBe(caseCompletedCommand.VendorId);
                legalEntity.VrfCaseStatus.Should().NotBe(caseCompletedCommand.Status);
                legalEntity.VrfCaseStatusLastUpdatedDateTime.Should().NotBe(caseCompletedCommand.CaseStatusLastUpdatedDate);
            }
        }

        [Test]
        public async Task Then_event_BankDetailsApprovedForLegalEntity_is_raised_when_status_is_completed()
        {
            // Arrange
            var caseCompletedCommand = new UpdateVendorRegistrationCaseStatusCommand(LegalEntityToBeUpdatedId, _fixture.Create<string>(), _fixture.Create<string>(), "Case request complete",
                _fixture.Create<DateTime>());

            var account = _fixture.Create<Account>();
            var legalEntityToBeUpdated = LegalEntity.New(123, _fixture.Create<string>(), LegalEntityToBeUpdatedId);

            account.AddLegalEntity(1, legalEntityToBeUpdated);
            account.AddLegalEntity(2, _fixture.Create<LegalEntity>());
            account.AddLegalEntity(3, _fixture.Create<LegalEntity>());

            _mockDomainRepository.Setup(x => x.GetByHashedLegalEntityId(caseCompletedCommand.HashedLegalEntityId)).ReturnsAsync(new List<Account> { account });
            await _sut.Handle(caseCompletedCommand);

            var @events = account.FlushEvents().ToList();

            @events.Count.Should().Be(1);
            @events[0].GetType().Should().Be<BankDetailsApprovedForLegalEntity>();
            ((BankDetailsApprovedForLegalEntity) @events[0]).HashedLegalEntityId.Should().Be(LegalEntityToBeUpdatedId);
        }
    }
}
