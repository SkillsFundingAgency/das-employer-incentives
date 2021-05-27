using System;
using System.Collections.Generic;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.RemoveLegalEntity;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using Account = SFA.DAS.EmployerIncentives.Domain.Accounts.Account;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;

namespace SFA.DAS.EmployerIncentives.Application.UnitTests.RemoveLegalEntity.Handlers
{
    [TestFixture]
    public class WhenHandlingRemoveLegalEntityCommand
    {
        private RemoveLegalEntityCommandHandler _sut;
        private Mock<IAccountDomainRepository> _accountDomainRepository;
        private Mock<IIncentiveApplicationDomainRepository> _applicationDomainRepository;
        private Mock<ICommandDispatcher> _commandDispatcher;
        
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _accountDomainRepository = new Mock<IAccountDomainRepository>();
            _applicationDomainRepository = new Mock<IIncentiveApplicationDomainRepository>();
            _commandDispatcher = new Mock<ICommandDispatcher>();
            
            _sut = new RemoveLegalEntityCommandHandler(_accountDomainRepository.Object, _applicationDomainRepository.Object, _commandDispatcher.Object);
        }

        [Test]
        public async Task Then_the_account_changes_are_persisted_to_the_domain_repository()
        {
            //Arrange
            var command = _fixture.Create<RemoveLegalEntityCommand>();
            var legalEntityModel = _fixture.Create<LegalEntityModel>();
            legalEntityModel.Id = command.AccountId;
            legalEntityModel.AccountLegalEntityId = command.AccountLegalEntityId;

            _accountDomainRepository
                .Setup(m => m.Find(command.AccountId))
                .ReturnsAsync(Account.Create(
                    new AccountModel
                    {
                        Id = command.AccountId,
                        LegalEntityModels = new Collection<LegalEntityModel>() {
                            _fixture.Create<LegalEntityModel>(),
                            legalEntityModel,
                            _fixture.Create<LegalEntityModel>(),
                        }
                    }));

            //Act
            await _sut.Handle(command);

            //Assert
            _accountDomainRepository.Verify(m => m.Save(It.Is<Account>(i => i.Id == command.AccountId && i.LegalEntities.Count == 2)), Times.Once);
        }

        [Test]
        public async Task Then_the_account_changes_are_not_persisted_to_the_domain_repository_if_it_does_not_exist()
        {
            //Arrange
            var command = _fixture.Create<RemoveLegalEntityCommand>();
            _accountDomainRepository
                .Setup(m => m.Find(command.AccountId))
                .ReturnsAsync(null as Account);

            //Act
            await _sut.Handle(command);

            //Assert
            _accountDomainRepository.Verify(m => m.Save(It.Is<Account>(i => i.Id == command.AccountId)), Times.Never);
        }

        [Test]
        public async Task Then_the_account_changes_are_not_persisted_to_the_domain_repository_if_the_account_exists_but_the_legalEntity_does_not_exist()
        {
            //Arrange
            var command = _fixture.Create<RemoveLegalEntityCommand>();
            _accountDomainRepository
                .Setup(m => m.Find(command.AccountId))
                .ReturnsAsync(Account.Create(new AccountModel { Id = 1, LegalEntityModels = new Collection<LegalEntityModel>() { _fixture.Create<LegalEntityModel>() } }));

            //Act
            await _sut.Handle(command);

            //Assert
            _accountDomainRepository.Verify(m => m.Save(It.Is<Account>(i => i.Id == command.AccountId)), Times.Never);
        }

        [Test]
        public async Task Then_any_applications_for_the_account_legal_entity_are_withdrawn()
        {
            //Arrange
            var command = _fixture.Create<RemoveLegalEntityCommand>();
            var legalEntityModel = _fixture.Create<LegalEntityModel>();
            legalEntityModel.Id = command.AccountId;
            legalEntityModel.AccountLegalEntityId = command.AccountLegalEntityId;

            _accountDomainRepository
                .Setup(m => m.Find(command.AccountId))
                .ReturnsAsync(Account.Create(
                    new AccountModel
                    {
                        Id = command.AccountId,
                        LegalEntityModels = new Collection<LegalEntityModel>() {
                            _fixture.Create<LegalEntityModel>(),
                            legalEntityModel
                        }
                    }));

            var applications = new List<IncentiveApplication>
            {
                IncentiveApplication.New(Guid.NewGuid(), command.AccountId, command.AccountLegalEntityId),
                IncentiveApplication.New(Guid.NewGuid(), command.AccountId, command.AccountLegalEntityId),
                IncentiveApplication.New(Guid.NewGuid(), command.AccountId, command.AccountLegalEntityId)
            };
            foreach(var application in applications)
            {
                application.SetApprenticeships(_fixture.CreateMany<Apprenticeship>(2));
            }

            _applicationDomainRepository.Setup(x => x.FindByAccountLegalEntity(command.AccountLegalEntityId))
                .ReturnsAsync(applications);

            //Act
            await _sut.Handle(command);

            //Assert
            _commandDispatcher.Verify(x => x.Send(It.Is<WithdrawCommand>(y => y.AccountId == command.AccountId), It.IsAny<CancellationToken>()), Times.Exactly(6));
            foreach(var application in applications)
            {
                foreach(var apprenticeship in application.Apprenticeships)
                {
                    _commandDispatcher.Verify(x => x.Send(It.Is<WithdrawCommand>(y => y.IncentiveApplicationApprenticeshipId == apprenticeship.Id), It.IsAny<CancellationToken>()), Times.Once);
                }
            }
        }
    }
}
