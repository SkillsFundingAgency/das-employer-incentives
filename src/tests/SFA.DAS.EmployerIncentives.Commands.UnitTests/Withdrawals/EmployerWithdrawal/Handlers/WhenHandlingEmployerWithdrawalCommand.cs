﻿using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.Withdrawals;
using SFA.DAS.EmployerIncentives.Commands.Withdrawals.EmployerWithdrawal;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.Accounts;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.Withdrawals.EmployerWithdrawal.Handlers
{
    public class WhenHandlingEmployerWithdrawalCommand
    {
        private EmployerWithdrawalCommandHandler _sut;
        private Mock<IIncentiveApplicationDomainRepository> _mockDomainRepository;
         
        private Fixture _fixture;
        private Mock<IAccountDomainRepository> _mockAccountDomainRepository;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize(new IncentiveApplicationCustomization());

            _mockDomainRepository = new Mock<IIncentiveApplicationDomainRepository>();
            _mockAccountDomainRepository = new Mock<IAccountDomainRepository>();
            
            _sut = new EmployerWithdrawalCommandHandler(_mockDomainRepository.Object, _mockAccountDomainRepository.Object);
        }

        [Test]
        public async Task Then_changes_to_the_application_are_persisted_to_the_domain_repository_for_matching_ULNs()
        {
            //Arrange
            var command = _fixture.Create<EmployerWithdrawalCommand>();

            var account = Account.New(command.AccountId);
            var legalEntity = _fixture.Create<LegalEntity>();
            account.AddLegalEntity(command.AccountLegalEntityId, legalEntity);
            _mockAccountDomainRepository.Setup(r => r.Find(It.Is<long>(x => x == command.AccountId))).ReturnsAsync(account);

            var apprenticeshipModel = _fixture
                .Build<ApprenticeshipModel>()
                .With(a => a.ULN, command.ULN)
                .Create();

            var incentiveApplicationModel = _fixture
                .Build<IncentiveApplicationModel>()
                .With(x => x.AccountId, command.AccountId)
                .With(x => x.AccountLegalEntityId, command.AccountLegalEntityId)
                .With(i => i.ApprenticeshipModels, 
                    new List<ApprenticeshipModel> {
                        _fixture.Create<ApprenticeshipModel>(),
                        apprenticeshipModel,
                        _fixture.Create<ApprenticeshipModel>(),
                    })
                .Create();

            var testApplication = new IncentiveApplicationFactory().GetExisting(incentiveApplicationModel.Id, incentiveApplicationModel);

            var applications = new List<IncentiveApplication>()
            {
                testApplication
            };

            _mockDomainRepository
                .Setup(m => m.Find(command.AccountLegalEntityId, command.ULN))
                .ReturnsAsync(applications);
          
            //Act
            await _sut.Handle(command);

            //Assert
            _mockDomainRepository
                .Verify(m => m.Save(It.Is<IncentiveApplication>(application =>
                 application.Id == testApplication.Id &&
                 application.Apprenticeships.Count(a => 
                    a.ULN == command.ULN &&
                    a.WithdrawnByEmployer) == 1)),
                 Times.Once);
        }

        [Test]
        public void Then_a_WithdrawalException_is_thrown_when_there_are_no_matching_apprenticeships()
        {
            //Arrange            
            var command = _fixture.Create<EmployerWithdrawalCommand>();

            _mockDomainRepository
                .Setup(m => m.Find(command.AccountId, command.ULN))
                .ReturnsAsync(new List<IncentiveApplication>());

            // Act
            Func<Task> action = async () => await _sut.Handle(command);

            //Assert
            action.Should()
                .Throw<WithdrawalException>()
                .WithMessage("Unable to handle Employer withdrawal command.*");           
        }
    }
}
