using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.LegalEntity;
using SFA.DAS.EmployerIncentives.Commands.UpdateVrfCaseStatusForLegalEntity;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Events;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.UpdateVrfCaseStatusForAccount.Handlers
{
    public class WhenHandlingUpdateVrfCaseDetailsForAccountCommand
    {
        private UpdateVrfCaseStatusForAccountCommandHandler _sut;
        private Mock<IAccountDomainRepository> _mockDomainRepository;
        private Fixture _fixture;
        private const string LegalEntityToBeUpdatedId = "XYZ123";

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize(new IncentiveApplicationCustomization());
            _mockDomainRepository = new Mock<IAccountDomainRepository>();
            _sut = new UpdateVrfCaseStatusForAccountCommandHandler(_mockDomainRepository.Object);
        }

        [Test]
        public async Task Then_VRF_details_for_given_legal_entity_are_updated()
        {
            // Arrange
            var account = _fixture.Create<Account>();
            var command = new UpdateVrfCaseStatusForAccountCommand(
                account.Id,
                LegalEntityToBeUpdatedId, _fixture.Create<string>(), _fixture.Create<string>(),
                _fixture.Create<DateTime>());

            var legalEntityToBeUpdated = LegalEntity.New(123, _fixture.Create<string>(), LegalEntityToBeUpdatedId);

            account.AddLegalEntity(1, legalEntityToBeUpdated);
            account.AddLegalEntity(2, _fixture.Create<LegalEntity>());
            account.AddLegalEntity(3, _fixture.Create<LegalEntity>());
            account.AddLegalEntity(4, legalEntityToBeUpdated);

            _mockDomainRepository.Setup(x => x.Find(command.AccountId)).ReturnsAsync(account);

            // Act
            await _sut.Handle(command);

            // Assert
            var updatedLegalEntities = account.LegalEntities.Where(
                e => e.HashedLegalEntityId == command.HashedLegalEntityId && e.VrfCaseStatus != "Completed").ToList();
            updatedLegalEntities.Should().HaveCount(2);

            foreach (var legalEntity in updatedLegalEntities)
            {
                legalEntity.VrfCaseId.Should().Be(command.CaseId);
                legalEntity.VrfCaseStatus.Should().Be(command.Status);
                legalEntity.VrfCaseStatusLastUpdatedDateTime.Should().Be(command.LastUpdatedDate);
            }

            _mockDomainRepository.Verify(m => m.Save(account), Times.Once);
            _mockDomainRepository.VerifyAll();
        }


        [Test]
        public async Task Then_VRF_details_for_given_legal_entity_are_not_updated_if_status_is_completed()
        {
            // Arrange
            var account = _fixture.Create<Account>();
            var caseCompletedCommand = new UpdateVrfCaseStatusForAccountCommand(account.Id,
                LegalEntityToBeUpdatedId, _fixture.Create<string>(), "case request completed",
                _fixture.Create<DateTime>());

            var legalEntityToBeUpdated = LegalEntity.New(123, _fixture.Create<string>(), LegalEntityToBeUpdatedId);

            account.AddLegalEntity(1, legalEntityToBeUpdated);
            account.AddLegalEntity(2, _fixture.Create<LegalEntity>());
            account.AddLegalEntity(3, _fixture.Create<LegalEntity>());

            // Act
            _mockDomainRepository.Setup(x => x.Find(caseCompletedCommand.AccountId)).ReturnsAsync(account);
            await _sut.Handle(caseCompletedCommand);

            var command2 = new UpdateVrfCaseStatusForAccountCommand(account.Id, LegalEntityToBeUpdatedId, caseCompletedCommand.CaseId, "New Status",
                _fixture.Create<DateTime>());

            // Act
            await _sut.Handle(command2);

            // Assert
            var legalEntities = account.LegalEntities.Where(
                e => e.HashedLegalEntityId == command2.HashedLegalEntityId).ToList();
            legalEntities.Should().NotBeEmpty();

            foreach (var legalEntity in legalEntities)
            {
                legalEntity.VrfCaseId.Should().Be(caseCompletedCommand.CaseId);
                legalEntity.VrfCaseStatus.Should().Be(caseCompletedCommand.Status);
                legalEntity.VrfCaseStatusLastUpdatedDateTime.Should().Be(caseCompletedCommand.LastUpdatedDate);
            }
        }

        [Test]
        public async Task Then_event_BankDetailsApprovedForLegalEntity_is_raised_when_status_is_completed()
        {
            // Arrange
            var account = _fixture.Create<Account>();
            var caseCompletedCommand = new UpdateVrfCaseStatusForAccountCommand(account.Id,
                LegalEntityToBeUpdatedId, _fixture.Create<string>(), LegalEntityVrfCaseStatus.Completed, _fixture.Create<DateTime>());

            var legalEntityToBeUpdated = LegalEntity.New(123, _fixture.Create<string>(), LegalEntityToBeUpdatedId);

            account.AddLegalEntity(1, legalEntityToBeUpdated);
            account.AddLegalEntity(2, _fixture.Create<LegalEntity>());
            account.AddLegalEntity(3, _fixture.Create<LegalEntity>());

            _mockDomainRepository.Setup(x => x.Find(caseCompletedCommand.AccountId)).ReturnsAsync(account);

            // Act
            await _sut.Handle(caseCompletedCommand);

            // Assert
            var events = account.FlushEvents().ToList();

            events.Count.Should().Be(1);
            events[0].GetType().Should().Be<BankDetailsApprovedForLegalEntity>();
            ((BankDetailsApprovedForLegalEntity)events[0]).HashedLegalEntityId.Should().Be(LegalEntityToBeUpdatedId);
        }
    }
}
