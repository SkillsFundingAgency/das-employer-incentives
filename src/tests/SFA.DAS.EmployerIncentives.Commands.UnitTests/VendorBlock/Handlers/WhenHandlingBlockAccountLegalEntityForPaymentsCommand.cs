using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.VendorBlock;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.VendorBlock.Handlers
{
    public class WhenHandlingBlockAccountLegalEntityForPaymentsCommand
    {
        private BlockAccountLegalEntityForPaymentsCommandHandler _sut;
        private Mock<IAccountDomainRepository> _mockDomainRepository;
        private Mock<IDomainEventDispatcher> _mockDomainEventDispatcher;
        private BlockAccountLegalEntityForPaymentsCommand _command;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockDomainRepository = new Mock<IAccountDomainRepository>();
            _mockDomainEventDispatcher = new Mock<IDomainEventDispatcher>();

            _sut = new BlockAccountLegalEntityForPaymentsCommandHandler(_mockDomainRepository.Object, _mockDomainEventDispatcher.Object);

            _command = new BlockAccountLegalEntityForPaymentsCommand(_fixture.Create<string>(),
                DateTime.Now.AddMonths(1), _fixture.Create<string>(), _fixture.Create<string>(),
                _fixture.Create<DateTime>());

        }

        [Test]
        public async Task Then_the_single_account_legal_entity_with_matching_vendor_id_is_updated_with_a_vendor_block_end_date()
        {
            // Arrange
            var accountModel = _fixture.Build<AccountModel>()
                .With(x => x.LegalEntityModels, new List<LegalEntityModel>()
                {
                    _fixture.Build<LegalEntityModel>()
                        .With(x => x.VrfVendorId, _command.VendorId)
                        .Create()
                })
                .Create();
            
            var accounts = new List<Account> { Account.Create(accountModel) };
            _mockDomainRepository.Setup(x => x.FindByVendorId(_command.VendorId)).ReturnsAsync(accounts);

            // Act
            await _sut.Handle(_command);

            // Assert
            _mockDomainRepository.Verify(x => x.Save(It.Is<Account>(
                    y => y.LegalEntities.ToList()[0].VendorBlockEndDate == _command.VendorBlockEndDate
                    && y.LegalEntities.ToList()[0].VrfVendorId == _command.VendorId)),
                    Times.Once);

            _mockDomainEventDispatcher.Verify(x => x.Send(It.Is<VendorBlockCreated>(
                y => y.VendorId == _command.VendorId
                && y.VendorBlockEndDate == _command.VendorBlockEndDate
                && y.ServiceRequest.TaskId == _command.ServiceRequestTaskId
                && y.ServiceRequest.DecisionReference == _command.ServiceRequestDecisionReference
                && y.ServiceRequest.Created == _command.ServiceRequestCreatedDate), 
                It.IsAny<CancellationToken>()), 
                Times.Once());
        }


        [Test]
        public async Task Then_multiple_account_legal_entities_with_matching_vendor_id_are_updated_with_a_vendor_block_end_date()
        {
            // Arrange
            var accountModel1 = _fixture.Build<AccountModel>()
                .With(x => x.LegalEntityModels, new List<LegalEntityModel>()
                {
                    _fixture.Build<LegalEntityModel>()
                        .With(x => x.AccountLegalEntityId, 123)
                        .With(x => x.VrfVendorId, _command.VendorId)
                        .Create()
                })
                .Create();
            var accountModel2 = _fixture.Build<AccountModel>()
                .With(x => x.LegalEntityModels, new List<LegalEntityModel>()
                {
                    _fixture.Build<LegalEntityModel>()
                        .With(x => x.AccountLegalEntityId, 123)
                        .With(x => x.VrfVendorId, _command.VendorId)
                        .Create()
                })
                .Create();

            var accounts = new List<Account> { Account.Create(accountModel1), Account.Create(accountModel2) };
            _mockDomainRepository.Setup(x => x.FindByVendorId(_command.VendorId)).ReturnsAsync(accounts);

            // Act
            await _sut.Handle(_command);

            // Assert
            _mockDomainRepository.Verify(x => x.Save(It.Is<Account>(
                    y => y.LegalEntities.ToList()[0].VendorBlockEndDate == _command.VendorBlockEndDate
                         && y.LegalEntities.ToList()[0].VrfVendorId == _command.VendorId)),
                Times.Exactly(2));

            _mockDomainEventDispatcher.Verify(x => x.Send(It.Is<VendorBlockCreated>(
                        y => y.VendorId == _command.VendorId
                             && y.VendorBlockEndDate == _command.VendorBlockEndDate
                             && y.ServiceRequest.TaskId == _command.ServiceRequestTaskId
                             && y.ServiceRequest.DecisionReference == _command.ServiceRequestDecisionReference
                             && y.ServiceRequest.Created == _command.ServiceRequestCreatedDate),
                    It.IsAny<CancellationToken>()),
                Times.Once());
        }

        [Test]
        public async Task Then_no_account_legal_entities_are_updated_if_none_match_the_supplied_vendor_id()
        {
            // Arrange
            var accounts = new List<Account>();
            _mockDomainRepository.Setup(x => x.FindByVendorId(_command.VendorId)).ReturnsAsync(accounts);

            // Act
            await _sut.Handle(_command);

            // Assert
            _mockDomainRepository.Verify(x => x.Save(It.Is<Account>(
                    y => y.LegalEntities.ToList()[0].VendorBlockEndDate == _command.VendorBlockEndDate
                         && y.LegalEntities.ToList()[0].VrfVendorId == _command.VendorId)),
                Times.Never);

            _mockDomainEventDispatcher.Verify(x => x.Send(It.Is<VendorBlockCreated>(
                        y => y.VendorId == _command.VendorId
                             && y.VendorBlockEndDate == _command.VendorBlockEndDate
                             && y.ServiceRequest.TaskId == _command.ServiceRequestTaskId
                             && y.ServiceRequest.DecisionReference == _command.ServiceRequestDecisionReference
                             && y.ServiceRequest.Created == _command.ServiceRequestCreatedDate),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
