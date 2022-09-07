﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.Withdrawals;
using SFA.DAS.EmployerIncentives.Commands.Withdrawals.ComplianceWithdrawal;
using SFA.DAS.EmployerIncentives.Data.IncentiveApplication;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.Withdrawals.ReinstateWithdrawal.Handlers
{
    public class WhenHandlingReinstateWithdrawalCommand
    {
        private ReinstateWithdrawalCommandHandler _sut;
        private Mock<IIncentiveApplicationDomainRepository> _mockDomainRepository;
        private Mock<IIncentiveApplicationStatusAuditDataRepository> _mockIncentiveApplicationStatusAuditDataRepository;


        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize(new IncentiveApplicationCustomization());

            _mockDomainRepository = new Mock<IIncentiveApplicationDomainRepository>();
            _mockIncentiveApplicationStatusAuditDataRepository = new Mock<IIncentiveApplicationStatusAuditDataRepository>();

            _sut = new ReinstateWithdrawalCommandHandler(_mockDomainRepository.Object, _mockIncentiveApplicationStatusAuditDataRepository.Object);
        }

        [Test]
        public async Task Then_changes_to_the_application_are_persisted_to_the_domain_repository_for_matching_ULNs()
        {
            //Arrange            
            var command = _fixture.Create<ReinstateWithdrawalCommand>();

            var apprenticeshipModel = _fixture
                .Build<ApprenticeshipModel>()
                .With(a => a.ULN, command.ULN)
                .With(a => a.WithdrawnByCompliance, true)
                .With(a => a.WithdrawnByEmployer, true)
                .Create();

            var incentiveApplicationModel = _fixture
                .Build<IncentiveApplicationModel>()
                .With(i => i.ApprenticeshipModels, 
                    new List<ApprenticeshipModel> {
                        _fixture.Create<ApprenticeshipModel>(),
                        apprenticeshipModel,
                        _fixture.Create<ApprenticeshipModel>(),
                    })
                .With(x => x.Status, IncentiveApplicationStatus.ComplianceWithdrawn)
                .Create();

            var lstIncentiveApplicationStatusAudit = _fixture.Build<Data.Models.IncentiveApplicationStatusAudit>()
                    .With(a => a.IncentiveApplicationApprenticeshipId, apprenticeshipModel.Id)
                    .With(a => a.Process, IncentiveApplicationStatus.ComplianceWithdrawn)
                    .CreateMany(2).ToList();

            var testApplication = new IncentiveApplicationFactory().GetExisting(incentiveApplicationModel.Id, incentiveApplicationModel);

            var applications = new List<IncentiveApplication>()
            {
                _fixture.Create<IncentiveApplication>(),
                testApplication,
                _fixture.Create<IncentiveApplication>()
            };

            _mockDomainRepository
                .Setup(m => m.Find(command.AccountLegalEntityId, command.ULN))
                .ReturnsAsync(applications);

            _mockIncentiveApplicationStatusAuditDataRepository.Setup(x => x.GetByApplicationApprenticeshipId(It.IsAny<Guid>())).Returns(lstIncentiveApplicationStatusAudit);
          
            //Act
            await _sut.Handle(command);

            //Assert
            _mockDomainRepository
                .Verify(m => m.Save(It.Is<IncentiveApplication>(a =>
                 a.Id == testApplication.Id &&
                 a.Apprenticeships.Count(a => 
                    a.ULN == command.ULN &&
                    !a.WithdrawnByCompliance &&
                    !a.WithdrawnByEmployer) == 1)),
                 Times.Once);
        }
        
        [Test]
        public void Then_applications_not_withdrawn_by_compliance_are_not_reinstated()
        {
            //Arrange            
            var command = _fixture.Create<ReinstateWithdrawalCommand>();

            var apprenticeshipModel = _fixture
                .Build<ApprenticeshipModel>()
                .With(a => a.ULN, command.ULN)
                .With(a => a.WithdrawnByCompliance, false)
                .With(a => a.WithdrawnByEmployer, true)
                .Create();

            var incentiveApplicationModel = _fixture
                .Build<IncentiveApplicationModel>()
                .With(i => i.ApprenticeshipModels,
                    new List<ApprenticeshipModel> {
                        _fixture.Create<ApprenticeshipModel>(),
                        apprenticeshipModel,
                        _fixture.Create<ApprenticeshipModel>(),
                    })
                .With(x => x.Status, IncentiveApplicationStatus.EmployerWithdrawn)
                .Create();

            var lstIncentiveApplicationStatusAudit = _fixture.Build<Data.Models.IncentiveApplicationStatusAudit>()
                    .With(a => a.IncentiveApplicationApprenticeshipId, apprenticeshipModel.Id)
                    .With(a => a.Process, IncentiveApplicationStatus.ComplianceWithdrawn)
                    .CreateMany(2).ToList();

            var testApplication = new IncentiveApplicationFactory().GetExisting(incentiveApplicationModel.Id, incentiveApplicationModel);

            var applications = new List<IncentiveApplication>()
            {
                _fixture.Create<IncentiveApplication>(),
                testApplication,
                _fixture.Create<IncentiveApplication>()
            };

            _mockDomainRepository
                .Setup(m => m.Find(command.AccountLegalEntityId, command.ULN))
                .ReturnsAsync(applications);

            _mockIncentiveApplicationStatusAuditDataRepository.Setup(x => x.GetByApplicationApprenticeshipId(It.IsAny<Guid>())).Returns(lstIncentiveApplicationStatusAudit);

            //Act
            Func<Task> action = async () => await _sut.Handle(command);

            //Assert
            action.Should()
                .Throw<WithdrawalException>()
                .WithMessage("Unable to handle reinstate withdrawal command.*");
        }

        [Test]
        public async Task Then_the_most_recent_application_is_reinstated_if_there_are_multiple_applications_withdrawn_by_compliance()
        {
            //Arrange            
            var command = _fixture.Create<ReinstateWithdrawalCommand>();

            var firstApprenticeshipModel = _fixture
                .Build<ApprenticeshipModel>()
                .With(a => a.ULN, command.ULN)
                .With(a => a.WithdrawnByCompliance, true)
                .With(a => a.WithdrawnByEmployer, false)
                .Create();

            var secondApprenticeshipModel = _fixture
                .Build<ApprenticeshipModel>()
                .With(a => a.ULN, command.ULN)
                .With(a => a.WithdrawnByCompliance, true)
                .With(a => a.WithdrawnByEmployer, false)
                .Create();

            var firstIncentiveApplicationModel = _fixture
                .Build<IncentiveApplicationModel>()
                .With(i => i.ApprenticeshipModels,
                    new List<ApprenticeshipModel> {
                        firstApprenticeshipModel
                    })
                .With(x => x.DateSubmitted, new DateTime(2021, 10, 01))
                .With(x => x.Status, IncentiveApplicationStatus.ComplianceWithdrawn)
                .Create();

            var secondIncentiveApplicationModel = _fixture
                .Build<IncentiveApplicationModel>()
                .With(i => i.ApprenticeshipModels,
                    new List<ApprenticeshipModel> {
                        secondApprenticeshipModel
                    })
                .With(x => x.DateSubmitted, new DateTime(2021, 11, 01))
                .With(x => x.Status, IncentiveApplicationStatus.ComplianceWithdrawn)
                .Create();
            
            var firstApplication = new IncentiveApplicationFactory().GetExisting(firstIncentiveApplicationModel.Id, firstIncentiveApplicationModel);
            var secondApplication = new IncentiveApplicationFactory().GetExisting(secondIncentiveApplicationModel.Id, secondIncentiveApplicationModel);

            var applications = new List<IncentiveApplication>
            {
                firstApplication,
                secondApplication
            };

            _mockDomainRepository
                .Setup(m => m.Find(command.AccountLegalEntityId, command.ULN))
                .ReturnsAsync(applications);

            var lstIncentiveApplicationStatusAudit = new List<Data.Models.IncentiveApplicationStatusAudit> 
            {
                _fixture.Build<Data.Models.IncentiveApplicationStatusAudit>()
                    .With(a => a.IncentiveApplicationApprenticeshipId, firstApprenticeshipModel.Id)
                    .With(a => a.Process, IncentiveApplicationStatus.ComplianceWithdrawn)
                    .Create(),
                _fixture.Build<Data.Models.IncentiveApplicationStatusAudit>()
                    .With(a => a.IncentiveApplicationApprenticeshipId, secondApprenticeshipModel.Id)
                    .With(a => a.Process, IncentiveApplicationStatus.ComplianceWithdrawn)
                    .Create()
            }; 
        
            _mockIncentiveApplicationStatusAuditDataRepository.Setup(x => x.GetByApplicationApprenticeshipId(It.IsAny<Guid>())).Returns(lstIncentiveApplicationStatusAudit);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockDomainRepository
                .Verify(m => m.Save(It.Is<IncentiveApplication>(a =>
                        a.Id == secondApplication.Id &&
                        a.Apprenticeships.Count(a =>
                            a.ULN == command.ULN &&
                            !a.WithdrawnByCompliance &&
                            !a.WithdrawnByEmployer) == 1)),
                    Times.Once);
        }

        [Test]
        public async Task Then_an_application_withdrawn_by_compliance_is_reinstated_if_another_application_withdrawn_by_employer_exists()
        {
            //Arrange            
            var command = _fixture.Create<ReinstateWithdrawalCommand>();

            var firstApprenticeshipModel = _fixture
                .Build<ApprenticeshipModel>()
                .With(a => a.ULN, command.ULN)
                .With(a => a.WithdrawnByCompliance, false)
                .With(a => a.WithdrawnByEmployer, true)
                .Create();

            var secondApprenticeshipModel = _fixture
                .Build<ApprenticeshipModel>()
                .With(a => a.ULN, command.ULN)
                .With(a => a.WithdrawnByCompliance, true)
                .With(a => a.WithdrawnByEmployer, false)
                .Create();

            var firstIncentiveApplicationModel = _fixture
                .Build<IncentiveApplicationModel>()
                .With(i => i.ApprenticeshipModels,
                    new List<ApprenticeshipModel> {
                        firstApprenticeshipModel
                    })
                .With(x => x.Status, IncentiveApplicationStatus.EmployerWithdrawn)
                .With(x => x.DateSubmitted, new DateTime(2021, 10, 01))
                .Create();

            var secondIncentiveApplicationModel = _fixture
                .Build<IncentiveApplicationModel>()
                .With(i => i.ApprenticeshipModels,
                    new List<ApprenticeshipModel> {
                        secondApprenticeshipModel
                    })
                .With(x => x.Status, IncentiveApplicationStatus.ComplianceWithdrawn)
                .With(x => x.DateSubmitted, new DateTime(2021, 11, 01))
                .Create();

            var firstApplication = new IncentiveApplicationFactory().GetExisting(firstIncentiveApplicationModel.Id, firstIncentiveApplicationModel);
            var secondApplication = new IncentiveApplicationFactory().GetExisting(secondIncentiveApplicationModel.Id, secondIncentiveApplicationModel);

            var applications = new List<IncentiveApplication>
            {
                firstApplication,
                secondApplication
            };

            _mockDomainRepository
                .Setup(m => m.Find(command.AccountLegalEntityId, command.ULN))
                .ReturnsAsync(applications);

            var lstIncentiveApplicationStatusAudit = new List<Data.Models.IncentiveApplicationStatusAudit>
            {
                _fixture.Build<Data.Models.IncentiveApplicationStatusAudit>()
                    .With(a => a.IncentiveApplicationApprenticeshipId, firstApprenticeshipModel.Id)
                    .With(a => a.Process, IncentiveApplicationStatus.EmployerWithdrawn)
                    .Create(),
                _fixture.Build<Data.Models.IncentiveApplicationStatusAudit>()
                    .With(a => a.IncentiveApplicationApprenticeshipId, secondApprenticeshipModel.Id)
                    .With(a => a.Process, IncentiveApplicationStatus.ComplianceWithdrawn)
                    .Create()
            };

            _mockIncentiveApplicationStatusAuditDataRepository.Setup(x => x.GetByApplicationApprenticeshipId(It.IsAny<Guid>())).Returns(lstIncentiveApplicationStatusAudit);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockDomainRepository
                .Verify(m => m.Save(It.Is<IncentiveApplication>(a =>
                        a.Id == secondApplication.Id &&
                        a.Apprenticeships.Count(a =>
                            a.ULN == command.ULN &&
                            !a.WithdrawnByCompliance &&
                            !a.WithdrawnByEmployer) == 1)),
                    Times.Once);
        }

        [Test]
        public void Then_a_WithdrawalException_is_thrown_when_there_are_no_matching_apprenticeships()
        {
            //Arrange            
            var command = _fixture.Create<ReinstateWithdrawalCommand>();

            _mockDomainRepository
                .Setup(m => m.Find(command.AccountLegalEntityId, command.ULN))
                .ReturnsAsync(new List<IncentiveApplication>());

            // Act
            Func<Task> action = async () => await _sut.Handle(command);

            //Assert
            action.Should()
                .Throw<WithdrawalException>()
                .WithMessage("Unable to handle reinstate withdrawal command.*");           
        }
    }
}
