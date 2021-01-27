using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.LegalEntity;
using SFA.DAS.EmployerIncentives.Commands.UpdateVrfCaseStatusForLegalEntity;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.UpdateVrfCaseStatusForLegalEntity.Handlers
{
    public class WhenHandlingUpdateVrfCaseDetailsForAccountCommand
    {
        private UpdateVendorRegistrationCaseStatusCommandHandler _sut;
        private Mock<IAccountDomainRepository> _mockDomainRepository;
        private Mock<ICommandPublisher> _mockCommandPublisher;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize(new IncentiveApplicationCustomization());

            _mockDomainRepository = new Mock<IAccountDomainRepository>();
            _mockCommandPublisher = new Mock<ICommandPublisher>();

            _sut = new UpdateVendorRegistrationCaseStatusCommandHandler(_mockCommandPublisher.Object, _mockDomainRepository.Object);
        }

        [Test]
        public async Task Then_UpdateVendorRegistrationCaseStatusForAccountCommand_is_published_for_each_matching_account()
        {
            // Arrange
            var command = new UpdateVendorRegistrationCaseStatusCommand(_fixture.Create<string>(),
                _fixture.Create<string>(), _fixture.Create<string>(),
                _fixture.Create<DateTime>());

            var accounts = _fixture.CreateMany<Account>().ToList();

            _mockDomainRepository.Setup(x => x.GetByHashedLegalEntityId(command.HashedLegalEntityId))
                .ReturnsAsync(accounts);

            // Act
            await _sut.Handle(command, CancellationToken.None);

            // Assert
            foreach (var account in accounts)
            {
                _mockCommandPublisher.Verify(x => x.Publish(It.Is<UpdateVendorRegistrationCaseStatusForAccountCommand>(
                    c =>
                        c.AccountId == account.Id &&
                        c.CaseId == command.CaseId &&
                        c.HashedLegalEntityId == command.HashedLegalEntityId &&
                        c.Status == command.Status &&
                        c.LastUpdatedDate == command.CaseStatusLastUpdatedDate
                ), CancellationToken.None), Times.Once());
            }
        }
    }
}
