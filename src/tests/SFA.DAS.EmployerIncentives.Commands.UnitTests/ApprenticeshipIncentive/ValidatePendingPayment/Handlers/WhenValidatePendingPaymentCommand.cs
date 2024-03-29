﻿using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.ValidatePendingPayment;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.ValidatePendingPayment.Handlers
{
    public class WhenValidatePendingPaymentCommand
    {
        private ValidatePendingPaymentCommandHandler _sut;
        private Mock<IApprenticeshipIncentiveDomainRepository> _mockIncentiveDomainRespository;
        private Mock<IAccountDomainRepository> _mockAccountDomainRepository;
        private Mock<ILearnerDomainRepository> _mockLearnerDomainRepository;
        private Mock<ICollectionCalendarService> _mockCollectionCalendarService;
        private Mock<IDateTimeService> _mockDateTimeService;
        private List<CollectionCalendarPeriod> _collectionCalendarPeriods;
        private string _vrfVendorId;
        private Account _account;
        private LearnerModel _learnerModel;
        private Learner _learner;
        private DaysInLearning _daysInLearning;
        private DateTime _startDate;
        private DateTime _payment1DueDate;
        private ApprenticeshipIncentiveModel _model;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockIncentiveDomainRespository = new Mock<IApprenticeshipIncentiveDomainRepository>();
            _mockCollectionCalendarService = new Mock<ICollectionCalendarService>();
            _mockAccountDomainRepository = new Mock<IAccountDomainRepository>();
            _mockLearnerDomainRepository = new Mock<ILearnerDomainRepository>();
            _mockDateTimeService = new Mock<IDateTimeService>();

            _startDate = DateTime.Today;
            _payment1DueDate = _startDate.AddDays(10);

            _collectionCalendarPeriods = new List<CollectionCalendarPeriod>()
            {
                new CollectionCalendarPeriod(
                    new Domain.ValueObjects.CollectionPeriod(1, (short)DateTime.Now.Year),
                    (byte)DateTime.Now.Month,
                    (short)DateTime.Now.Year,
                    DateTime.Now.AddDays(-1),
                    DateTime.Now,
                    true,
                    false)
            };

            _mockCollectionCalendarService
                .Setup(m => m.Get())
                .ReturnsAsync(new Domain.ValueObjects.CollectionCalendar(new List<AcademicYear>(), _collectionCalendarPeriods));

            _vrfVendorId = Guid.NewGuid().ToString();
            var legalEntity = _fixture.Build<LegalEntityModel>()
                .With(l => l.VrfVendorId, _vrfVendorId)
                .With(l => l.SignedAgreementVersion, 5)
                .Without(l => l.VendorBlockEndDate)
                .Create();

            var accountModel = _fixture.Build<AccountModel>()
               .With(a => a.LegalEntityModels, new List<LegalEntityModel>() { legalEntity })
               .Create();

            var domainAccount = Domain.Accounts.Account.Create(accountModel);
            _account = new Account(accountModel.Id, legalEntity.AccountLegalEntityId);

            var pendingPayment1 = _fixture.Build<PendingPaymentModel>()
                .With(m => m.Account, _account)
                .With(m => m.DueDate, _payment1DueDate)
                .With(m => m.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create();

            var pendingPayment2 = _fixture.Build<PendingPaymentModel>()
                .With(m => m.Account, _account)
                .With(m => m.DueDate, _payment1DueDate.AddDays(2))
                .With(m => m.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create();

            var pendingPayments = new List<PendingPaymentModel>() { pendingPayment1, pendingPayment2 };

            _model = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(m => m.Account, _account)
                .With(m => m.StartDate, _startDate)
                .With(m => m.PendingPaymentModels, pendingPayments)
                .With(m => m.PausePayments, false)
                .With(m => m.MinimumAgreementVersion, new AgreementVersion(4))
                .With(m => m.Phase, new IncentivePhase(Phase.Phase2))
                .With(m => m.EmploymentCheckModels, new List<EmploymentCheckModel>
                {
                    _fixture.Build<EmploymentCheckModel>()
                        .With(x => x.CheckType, EmploymentCheckType.EmployedAtStartOfApprenticeship)
                        .With(x => x.Result, true)
                        .Create(),
                    _fixture.Build<EmploymentCheckModel>()
                        .With(x => x.CheckType, EmploymentCheckType.EmployedBeforeSchemeStarted)
                        .With(x => x.Result, false)
                        .Create()
                })
                .Without(m => m.ValidationOverrideModels)
                .Create();

            var incentive = new ApprenticeshipIncentiveFactory().GetExisting(_model.Id, _model);

            _fixture.Register(() => incentive);

            _mockIncentiveDomainRespository
                .Setup(m => m.Find(incentive.Id))
                .ReturnsAsync(incentive);

            _mockAccountDomainRepository
                .Setup(m => m.Find(incentive.Account.Id))
                .ReturnsAsync(domainAccount);

            var submissionData = new SubmissionData();
            submissionData.SetSubmissionDate(DateTime.UtcNow);
            submissionData.SetLearningData(new LearningData(true));
            submissionData.LearningData.SetIsInLearning(true);

            _daysInLearning = new DaysInLearning(new Domain.ValueObjects.CollectionPeriod(1, (short)DateTime.Now.Year), 90);

            _learnerModel = _fixture.Build<LearnerModel>()
                .With(x => x.SuccessfulLearnerMatch, true)
                .With(m => m.ApprenticeshipId, incentive.Apprenticeship.Id)
                .With(m => m.ApprenticeshipIncentiveId, incentive.Id)
                .With(m => m.UniqueLearnerNumber, incentive.Apprenticeship.UniqueLearnerNumber)
                .With(m => m.SubmissionData, submissionData)
                .With(m => m.DaysInLearnings, new List<DaysInLearning>() { _daysInLearning })
                .Without(l => l.LearningPeriods)
                .Create();

            _learner = new LearnerFactory().GetExisting(_learnerModel);

            _mockLearnerDomainRepository
                .Setup(m => m.GetOrCreate(incentive))
                .ReturnsAsync(_learner);

            _sut = new ValidatePendingPaymentCommandHandler(
                _mockIncentiveDomainRespository.Object,
                _mockAccountDomainRepository.Object,
                _mockCollectionCalendarService.Object,
                _mockLearnerDomainRepository.Object,
                _mockDateTimeService.Object);
        }

        [Test]
        public async Task Then_a_passing_pendingPayment_validation_result_is_saved_against_the_pendingPayment_and_the_pending_payment_is_validated()
        {
            //Arrange
            var incentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();

            var pendingPayment = incentive.PendingPayments.First();
            var collectionPeriod = _collectionCalendarPeriods.First().CollectionPeriod;

            var command = new ValidatePendingPaymentCommand(incentive.Id, pendingPayment.Id, collectionPeriod.AcademicYear, collectionPeriod.PeriodNumber);

            // Act
            await _sut.Handle(command);

            // Assert
            incentive.PendingPayments.Count(p => p.PendingPaymentValidationResults.Count >= 1).Should().Be(1);
            incentive.PendingPayments.First().IsValidated(new Domain.ValueObjects.CollectionPeriod(collectionPeriod.PeriodNumber, collectionPeriod.AcademicYear)).Should().BeTrue();
        }

        [Test]
        public void Then_a_pendingPayment_is_not_valid_if_no_validations_have_been_run()
        {
            //Arrange
            var collectionPeriod = _collectionCalendarPeriods.First().CollectionPeriod;
            var incentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();

            // Assert
            incentive.PendingPayments.Count(p => p.PendingPaymentValidationResults.Count >= 1).Should().Be(0);
            incentive.PendingPayments.First().IsValidated(collectionPeriod).Should().BeFalse();
        }

        [Test]
        public async Task Then_a_failed_pendingPayment_validation_result_is_saved_against_the_pendingPayment_and_the_pending_payment_is_not_validated()
        {
            //Arrange
            var incentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();

            var pendingPayment = incentive.PendingPayments.First();
            var collectionPeriod = _collectionCalendarPeriods.First().CollectionPeriod;

            var accountModel = _fixture.Build<AccountModel>()
                .With(a => a.Id, _account.Id)
                .With(a => a.LegalEntityModels, new List<LegalEntityModel>() {
                   _fixture.Build<LegalEntityModel>()
                   .With(l => l.VrfVendorId, string.Empty)
                   .With(l => l.AccountLegalEntityId, _account.AccountLegalEntityId)
                   .Create()})
                .Create();

            var domainAccount = Domain.Accounts.Account.Create(accountModel);

            _mockAccountDomainRepository
               .Setup(m => m.Find(incentive.Account.Id))
               .ReturnsAsync(domainAccount);

            var command = new ValidatePendingPaymentCommand(incentive.Id, pendingPayment.Id, collectionPeriod.AcademicYear, collectionPeriod.PeriodNumber);

            // Act
            await _sut.Handle(command);

            // Assert
            incentive.PendingPayments.Count(p => p.PendingPaymentValidationResults.Count >= 1).Should().Be(1);
            incentive.PendingPayments.First().IsValidated(new Domain.ValueObjects.CollectionPeriod(collectionPeriod.PeriodNumber, collectionPeriod.AcademicYear)).Should().BeFalse();
        }

        [Test]
        public async Task Then_a_previously_failed_pendingPayment_validation_result_is_updated_to_passing()
        {
            //Arrange
            var incentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();

            var pendingPayment = incentive.PendingPayments.First();
            var collectionPeriod = _collectionCalendarPeriods.First().CollectionPeriod;

            var accountModel = _fixture.Build<AccountModel>()
                .With(a => a.Id, _account.Id)
                .With(a => a.LegalEntityModels, new List<LegalEntityModel>() {
                   _fixture.Build<LegalEntityModel>()
                   .With(l => l.VrfVendorId, string.Empty)
                   .With(l => l.AccountLegalEntityId, _account.AccountLegalEntityId)
                   .Create()})
                .Create();

            var domainAccount = Domain.Accounts.Account.Create(accountModel);

            _mockAccountDomainRepository
               .Setup(m => m.Find(incentive.Account.Id))
               .ReturnsAsync(domainAccount);

            var command = new ValidatePendingPaymentCommand(incentive.Id, pendingPayment.Id, collectionPeriod.AcademicYear, collectionPeriod.PeriodNumber);
            await _sut.Handle(command);
            incentive.PendingPayments.First().IsValidated(collectionPeriod).Should().BeFalse();

            // Act
            accountModel = _fixture.Build<AccountModel>()
                .With(a => a.Id, _account.Id)
                .With(a => a.LegalEntityModels, new List<LegalEntityModel>() {
                   _fixture.Build<LegalEntityModel>()
                   .With(l => l.VrfVendorId, Guid.NewGuid().ToString())
                   .With(l => l.AccountLegalEntityId, _account.AccountLegalEntityId)
                   .Without(l => l.VendorBlockEndDate)
                   .Create()})
                .Create();

            domainAccount = Domain.Accounts.Account.Create(accountModel);

            _mockAccountDomainRepository
               .Setup(m => m.Find(incentive.Account.Id))
               .ReturnsAsync(domainAccount);

            await _sut.Handle(command);

            // Assert
            incentive.PendingPayments.Count(p => p.PendingPaymentValidationResults.Count >= 1).Should().Be(1);
            incentive.PendingPayments.First().IsValidated(new Domain.ValueObjects.CollectionPeriod(collectionPeriod.PeriodNumber, collectionPeriod.AcademicYear)).Should().BeTrue();
        }


        [Test]
        public async Task Then_a_failed_pendingPayment_validation_result_is_saved_and_no_further_validation_performed_when_no_ILR_submission_found()
        {
            // Arrange
            var incentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();
            _learner.SetSubmissionData(null);
            Assert.IsFalse(_learner.SubmissionData.SubmissionFound);

            var pendingPayment = incentive.PendingPayments.First();
            var collectionPeriod = _collectionCalendarPeriods.First().CollectionPeriod;

            var accountModel = _fixture.Build<AccountModel>()
                .With(a => a.Id, _account.Id)
                .With(a => a.LegalEntityModels, new List<LegalEntityModel>() {
                    _fixture.Build<LegalEntityModel>()
                        .With(l => l.VrfVendorId, "VENDORID")
                        .With(l => l.AccountLegalEntityId, _account.AccountLegalEntityId)
                        .Create()})
                .Create();

            var domainAccount = Domain.Accounts.Account.Create(accountModel);

            _mockAccountDomainRepository
                .Setup(m => m.Find(incentive.Account.Id))
                .ReturnsAsync(domainAccount);

            var command = new ValidatePendingPaymentCommand(incentive.Id, pendingPayment.Id, collectionPeriod.AcademicYear, collectionPeriod.PeriodNumber);

            // Act
            await _sut.Handle(command);

            // Assert
            var validationResult = incentive.PendingPayments.Single(x => x.PendingPaymentValidationResults.Count == 8)
                .PendingPaymentValidationResults.Single(x => x.Step == "HasIlrSubmission");
            validationResult.Result.Should().BeFalse();
        }

        [Test]
        public async Task Then_a_failed_pendingPayment_validation_result_is_saved_and_no_further_validation_performed_when_learner_match_was_unsuccessful()
        {
            // Arrange
            var incentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();
            _learner.SetSuccessfulLearnerMatch(false);

            var pendingPayment = incentive.PendingPayments.First();
            var collectionPeriod = _collectionCalendarPeriods.First().CollectionPeriod;

            var accountModel = _fixture.Build<AccountModel>()
                .With(a => a.Id, _account.Id)
                .With(a => a.LegalEntityModels, new List<LegalEntityModel>() {
                    _fixture.Build<LegalEntityModel>()
                        .With(l => l.VrfVendorId, "VENDORID")
                        .With(l => l.AccountLegalEntityId, _account.AccountLegalEntityId)
                        .Create()})
                .Create();

            var domainAccount = Domain.Accounts.Account.Create(accountModel);

            _mockAccountDomainRepository
                .Setup(m => m.Find(incentive.Account.Id))
                .ReturnsAsync(domainAccount);

            var command = new ValidatePendingPaymentCommand(incentive.Id, pendingPayment.Id, collectionPeriod.AcademicYear, collectionPeriod.PeriodNumber);

            // Act
            await _sut.Handle(command);

            // Assert
            var validationResult = incentive.PendingPayments.Single(x => x.PendingPaymentValidationResults.Count == 7)
                .PendingPaymentValidationResults.Single(x => x.Step == "LearnerMatchSuccessful");
            validationResult.Result.Should().BeFalse();
        }

        [Test]
        public async Task Then_a_failed_pendingPayment_validation_result_is_saved_when_learner_was_employed_prior_to_scheme_start_date()
        {
            // Arrange
            var pendingPayment1 = _fixture.Build<PendingPaymentModel>()
                .With(m => m.Account, _account)
                .With(m => m.DueDate, _payment1DueDate)
                .With(m => m.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create();

            var pendingPayment2 = _fixture.Build<PendingPaymentModel>()
                .With(m => m.Account, _account)
                .With(m => m.DueDate, _payment1DueDate.AddDays(2))
                .With(m => m.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create();

            var pendingPayments = new List<PendingPaymentModel>() { pendingPayment1, pendingPayment2 };

            var model = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(m => m.Account, _account)
                .With(m => m.StartDate, _startDate)
                .With(m => m.PendingPaymentModels, pendingPayments)
                .With(m => m.PausePayments, false)
                .With(m => m.MinimumAgreementVersion, new AgreementVersion(4))
                .With(m => m.Phase, new IncentivePhase(Phase.Phase2))
                .With(m => m.EmploymentCheckModels, new List<EmploymentCheckModel>
                {
                    _fixture.Build<EmploymentCheckModel>()
                        .With(x => x.CheckType, EmploymentCheckType.EmployedBeforeSchemeStarted)
                        .With(x => x.Result, true)
                        .Create()
                })
                .Create();

            var incentive = new ApprenticeshipIncentiveFactory().GetExisting(model.Id, model);

            _mockIncentiveDomainRespository
                .Setup(m => m.Find(incentive.Id))
                .ReturnsAsync(incentive);

            _mockLearnerDomainRepository
                .Setup(m => m.GetOrCreate(incentive))
                .ReturnsAsync(_learner);

            var pendingPayment = incentive.PendingPayments.First();
            var collectionPeriod = _collectionCalendarPeriods.First().CollectionPeriod;

            var command = new ValidatePendingPaymentCommand(incentive.Id, pendingPayment.Id, collectionPeriod.AcademicYear, collectionPeriod.PeriodNumber);

            // Act
            await _sut.Handle(command);

            // Assert
            var validationResult = incentive.PendingPayments.Single(x => x.Id == pendingPayment.Id)
                .PendingPaymentValidationResults.Single(x => x.Step == "EmployedBeforeSchemeStarted");
            validationResult.Result.Should().BeFalse();
        }

        [Test]
        public async Task Then_a_failed_pendingPayment_validation_result_is_saved_when_learner_was_not_employed_on_or_after_scheme_start_date()
        {
            // Arrange
            var pendingPayment1 = _fixture.Build<PendingPaymentModel>()
                .With(m => m.Account, _account)
                .With(m => m.DueDate, _payment1DueDate)
                .With(m => m.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create();

            var pendingPayment2 = _fixture.Build<PendingPaymentModel>()
                .With(m => m.Account, _account)
                .With(m => m.DueDate, _payment1DueDate.AddDays(2))
                .With(m => m.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create();

            var pendingPayments = new List<PendingPaymentModel>() { pendingPayment1, pendingPayment2 };

            var model = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(m => m.Account, _account)
                .With(m => m.StartDate, _startDate)
                .With(m => m.PendingPaymentModels, pendingPayments)
                .With(m => m.PausePayments, false)
                .With(m => m.MinimumAgreementVersion, new AgreementVersion(4))
                .With(m => m.Phase, new IncentivePhase(Phase.Phase2))
                .With(m => m.EmploymentCheckModels, new List<EmploymentCheckModel>
                {
                    _fixture.Build<EmploymentCheckModel>()
                        .With(x => x.CheckType, EmploymentCheckType.EmployedAtStartOfApprenticeship)
                        .With(x => x.Result, false)
                        .Create()
                })
                .Create();

            var incentive = new ApprenticeshipIncentiveFactory().GetExisting(model.Id, model);

            _mockIncentiveDomainRespository
                .Setup(m => m.Find(incentive.Id))
                .ReturnsAsync(incentive);

            _mockLearnerDomainRepository
                .Setup(m => m.GetOrCreate(incentive))
                .ReturnsAsync(_learner);

            var pendingPayment = incentive.PendingPayments.First();
            var collectionPeriod = _collectionCalendarPeriods.First().CollectionPeriod;

            var command = new ValidatePendingPaymentCommand(incentive.Id, pendingPayment.Id, collectionPeriod.AcademicYear, collectionPeriod.PeriodNumber);

            // Act
            await _sut.Handle(command);

            // Assert
            var validationResult = incentive.PendingPayments.Single(x => x.Id == pendingPayment.Id)
                .PendingPaymentValidationResults.Single(x => x.Step == "EmployedAtStartOfApprenticeship");
            validationResult.Result.Should().BeFalse();
        }

        [Test]
        public async Task Then_an_expired_validation_override_is_removed()
        {
            //Arrange
            var expiredValidationOverride = _fixture
                  .Build<ValidationOverrideModel>()
                  .With(p => p.ApprenticeshipIncentiveId, _model.Id)
                  .With(p => p.ExpiryDate, DateTime.UtcNow.AddDays(-1))
                  .Create();

            _model.ValidationOverrideModels.Add(expiredValidationOverride);
            var incentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();

            var pendingPayment = incentive.PendingPayments.First();
            var collectionPeriod = _collectionCalendarPeriods.First().CollectionPeriod;

            var command = new ValidatePendingPaymentCommand(incentive.Id, pendingPayment.Id, collectionPeriod.AcademicYear, collectionPeriod.PeriodNumber);

            // Act
            await _sut.Handle(command);

            // Assert
            incentive.GetModel().ValidationOverrideModels.Count.Should().Be(0);
        }

        [Test]
        public async Task Then_a_failed_pendingPayment_validation_result_is_saved_when_the_account_has_a_vendor_block_end_date_in_the_future()
        {
            // Arrange
            var legalEntity = _fixture.Build<LegalEntityModel>()
                .With(l => l.VrfVendorId, _vrfVendorId)
                .With(l => l.SignedAgreementVersion, 5)
                .With(l => l.VendorBlockEndDate, DateTime.Now.AddDays(1))
                .Create();

            var accountModel = _fixture.Build<AccountModel>()
                .With(a => a.LegalEntityModels, new List<LegalEntityModel>() { legalEntity })
                .Create();

            var domainAccount = Domain.Accounts.Account.Create(accountModel);
            var account = new Account(accountModel.Id, legalEntity.AccountLegalEntityId);

            _mockAccountDomainRepository.Setup(x => x.Find(accountModel.Id)).ReturnsAsync(domainAccount);

            var pendingPayment1 = _fixture.Build<PendingPaymentModel>()
                .With(m => m.Account, account)
                .With(m => m.DueDate, _payment1DueDate)
                .With(m => m.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create();

            var pendingPayment2 = _fixture.Build<PendingPaymentModel>()
                .With(m => m.Account, account)
                .With(m => m.DueDate, _payment1DueDate.AddDays(2))
                .With(m => m.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create();

            var pendingPayments = new List<PendingPaymentModel>() { pendingPayment1, pendingPayment2 };

            var model = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(m => m.Account, account)
                .With(m => m.StartDate, _startDate)
                .With(m => m.PendingPaymentModels, pendingPayments)
                .With(m => m.PausePayments, false)
                .With(m => m.MinimumAgreementVersion, new AgreementVersion(4))
                .With(m => m.Phase, new IncentivePhase(Phase.Phase2))
                .With(m => m.EmploymentCheckModels, new List<EmploymentCheckModel>
                {
                    _fixture.Build<EmploymentCheckModel>()
                        .With(x => x.CheckType, EmploymentCheckType.EmployedAtStartOfApprenticeship)
                        .With(x => x.Result, false)
                        .Create()
                })
                .Create();

            var incentive = new ApprenticeshipIncentiveFactory().GetExisting(model.Id, model);

            _mockIncentiveDomainRespository
                .Setup(m => m.Find(incentive.Id))
                .ReturnsAsync(incentive);

            _mockLearnerDomainRepository
                .Setup(m => m.GetOrCreate(incentive))
                .ReturnsAsync(_learner);

            var pendingPayment = incentive.PendingPayments.First();
            var collectionPeriod = _collectionCalendarPeriods.First().CollectionPeriod;

            var command = new ValidatePendingPaymentCommand(incentive.Id, pendingPayment.Id, collectionPeriod.AcademicYear, collectionPeriod.PeriodNumber);

            // Act
            await _sut.Handle(command);

            // Assert
            var validationResult = incentive.PendingPayments.Single(x => x.Id == pendingPayment.Id)
                .PendingPaymentValidationResults.Single(x => x.Step == "BlockedForPayments");
            validationResult.Result.Should().BeFalse();
        }

        [Test]
        public async Task Then_a_passing_pendingPayment_validation_result_is_saved_when_the_account_has_no_vendor_block_end_date()
        {
            // Arrange
            var legalEntity = _fixture.Build<LegalEntityModel>()
                .With(l => l.VrfVendorId, _vrfVendorId)
                .With(l => l.SignedAgreementVersion, 5)
                .Without(l => l.VendorBlockEndDate)
                .Create();

            var accountModel = _fixture.Build<AccountModel>()
                .With(a => a.LegalEntityModels, new List<LegalEntityModel>() { legalEntity })
                .Create();

            var domainAccount = Domain.Accounts.Account.Create(accountModel);
            var account = new Account(accountModel.Id, legalEntity.AccountLegalEntityId);

            _mockAccountDomainRepository.Setup(x => x.Find(accountModel.Id)).ReturnsAsync(domainAccount);

            var pendingPayment1 = _fixture.Build<PendingPaymentModel>()
                .With(m => m.Account, account)
                .With(m => m.DueDate, _payment1DueDate)
                .With(m => m.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create();

            var pendingPayment2 = _fixture.Build<PendingPaymentModel>()
                .With(m => m.Account, account)
                .With(m => m.DueDate, _payment1DueDate.AddDays(2))
                .With(m => m.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create();

            var pendingPayments = new List<PendingPaymentModel>() { pendingPayment1, pendingPayment2 };

            var model = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(m => m.Account, account)
                .With(m => m.StartDate, _startDate)
                .With(m => m.PendingPaymentModels, pendingPayments)
                .With(m => m.PausePayments, false)
                .With(m => m.MinimumAgreementVersion, new AgreementVersion(4))
                .With(m => m.Phase, new IncentivePhase(Phase.Phase2))
                .With(m => m.EmploymentCheckModels, new List<EmploymentCheckModel>
                {
                    _fixture.Build<EmploymentCheckModel>()
                        .With(x => x.CheckType, EmploymentCheckType.EmployedAtStartOfApprenticeship)
                        .With(x => x.Result, false)
                        .Create()
                })
                .Create();

            var incentive = new ApprenticeshipIncentiveFactory().GetExisting(model.Id, model);

            _mockIncentiveDomainRespository
                .Setup(m => m.Find(incentive.Id))
                .ReturnsAsync(incentive);

            _mockLearnerDomainRepository
                .Setup(m => m.GetOrCreate(incentive))
                .ReturnsAsync(_learner);

            var pendingPayment = incentive.PendingPayments.First();
            var collectionPeriod = _collectionCalendarPeriods.First().CollectionPeriod;

            var command = new ValidatePendingPaymentCommand(incentive.Id, pendingPayment.Id, collectionPeriod.AcademicYear, collectionPeriod.PeriodNumber);

            // Act
            await _sut.Handle(command);

            // Assert
            var validationResult = incentive.PendingPayments.Single(x => x.Id == pendingPayment.Id)
                .PendingPaymentValidationResults.Single(x => x.Step == "BlockedForPayments");
            validationResult.Result.Should().BeTrue();
        }

        [Test]
        public async Task Then_a_passing_pendingPayment_validation_result_is_saved_when_the_account_has_a_vendor_block_end_date_in_the_past()
        {
            // Arrange
            var legalEntity = _fixture.Build<LegalEntityModel>()
                .With(l => l.VrfVendorId, _vrfVendorId)
                .With(l => l.SignedAgreementVersion, 5)
                .With(l => l.VendorBlockEndDate, DateTime.Now.AddDays(-1))
                .Create();

            var accountModel = _fixture.Build<AccountModel>()
                .With(a => a.LegalEntityModels, new List<LegalEntityModel>() { legalEntity })
                .Create();

            var domainAccount = Domain.Accounts.Account.Create(accountModel);
            var account = new Account(accountModel.Id, legalEntity.AccountLegalEntityId);

            _mockAccountDomainRepository.Setup(x => x.Find(accountModel.Id)).ReturnsAsync(domainAccount);

            var pendingPayment1 = _fixture.Build<PendingPaymentModel>()
                .With(m => m.Account, account)
                .With(m => m.DueDate, _payment1DueDate)
                .With(m => m.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create();

            var pendingPayment2 = _fixture.Build<PendingPaymentModel>()
                .With(m => m.Account, account)
                .With(m => m.DueDate, _payment1DueDate.AddDays(2))
                .With(m => m.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create();

            var pendingPayments = new List<PendingPaymentModel>() { pendingPayment1, pendingPayment2 };

            var model = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(m => m.Account, account)
                .With(m => m.StartDate, _startDate)
                .With(m => m.PendingPaymentModels, pendingPayments)
                .With(m => m.PausePayments, false)
                .With(m => m.MinimumAgreementVersion, new AgreementVersion(4))
                .With(m => m.Phase, new IncentivePhase(Phase.Phase2))
                .With(m => m.EmploymentCheckModels, new List<EmploymentCheckModel>
                {
                    _fixture.Build<EmploymentCheckModel>()
                        .With(x => x.CheckType, EmploymentCheckType.EmployedAtStartOfApprenticeship)
                        .With(x => x.Result, false)
                        .Create()
                })
                .Create();

            var incentive = new ApprenticeshipIncentiveFactory().GetExisting(model.Id, model);

            _mockIncentiveDomainRespository
                .Setup(m => m.Find(incentive.Id))
                .ReturnsAsync(incentive);

            _mockLearnerDomainRepository
                .Setup(m => m.GetOrCreate(incentive))
                .ReturnsAsync(_learner);

            var pendingPayment = incentive.PendingPayments.First();
            var collectionPeriod = _collectionCalendarPeriods.First().CollectionPeriod;

            var command = new ValidatePendingPaymentCommand(incentive.Id, pendingPayment.Id, collectionPeriod.AcademicYear, collectionPeriod.PeriodNumber);

            // Act
            await _sut.Handle(command);

            // Assert
            var validationResult = incentive.PendingPayments.Single(x => x.Id == pendingPayment.Id)
                .PendingPaymentValidationResults.Single(x => x.Step == "BlockedForPayments");
            validationResult.Result.Should().BeTrue();
        }        
    }
}
