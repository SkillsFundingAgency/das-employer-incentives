using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Data.IncentiveApplication;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Events;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using SFA.DAS.EmployerIncentives.Events.Accounts;

namespace SFA.DAS.EmployerIncentives.Events.UnitTests.Accounts
{
    [TestFixture]
    public class WhenHandlingTheAccountLegalEntityRemovedEvent
    {
        private Mock<IIncentiveApplicationDataRepository> _applicationRepository;
        private Mock<ICommandPublisher> _commandPublisher;
        private AccountLegalEntityRemovedHandler _sut;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _applicationRepository = new Mock<IIncentiveApplicationDataRepository>();
            _commandPublisher = new Mock<ICommandPublisher>();
            _sut = new AccountLegalEntityRemovedHandler(_applicationRepository.Object, _commandPublisher.Object);
            _fixture = new Fixture();
        }

        [Test]
        public async Task Then_any_applications_for_the_removed_account_legal_entity_are_withdrawn()
        {
            //Arrange
            var accountLegalEntityRemovedEvent = _fixture.Create<AccountLegalEntityRemoved>();

            var applications = _fixture.CreateMany<IncentiveApplicationModel>(6).ToList();
            foreach(var application in applications)
            {
                application.AccountLegalEntityId = accountLegalEntityRemovedEvent.AccountLegalEntityId;
            }
            _applicationRepository.Setup(x => x.FindApplicationsByAccountLegalEntity(accountLegalEntityRemovedEvent.AccountLegalEntityId))
                .ReturnsAsync(applications);

            //Act
            await _sut.Handle(accountLegalEntityRemovedEvent);

            //Assert
            _commandPublisher.Verify(x => x.Publish(It.IsAny<WithdrawCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(applications.Sum(x => x.ApprenticeshipModels.Count)));
            foreach (var application in applications)
            {
                foreach (var apprenticeship in application.ApprenticeshipModels)
                {
                    _commandPublisher.Verify(x => x.Publish(It.Is<WithdrawCommand>(y => y.IncentiveApplicationApprenticeshipId == apprenticeship.Id), It.IsAny<CancellationToken>()), Times.Once);
                }
            }
        }
    }
}
