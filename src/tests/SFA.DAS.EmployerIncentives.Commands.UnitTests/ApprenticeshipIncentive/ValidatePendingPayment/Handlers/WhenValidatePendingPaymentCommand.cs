using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.ValidatePendingPayment;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Services;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
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
        private List<Domain.ValueObjects.CollectionPeriod> _collectionPeriods;
        private string _vrfVendorId;
        private Account _account;
        private LearnerModel _learnerModel;
        private Learner _learner;
        private DaysInLearning _daysInLearning;
        private DateTime _startDate;
        private DateTime _payment1DueDate;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockIncentiveDomainRespository = new Mock<IApprenticeshipIncentiveDomainRepository>();
            _mockCollectionCalendarService = new Mock<ICollectionCalendarService>();
            _mockAccountDomainRepository = new Mock<IAccountDomainRepository>();
            _mockLearnerDomainRepository = new Mock<ILearnerDomainRepository>();

            _startDate = DateTime.Today;
            _payment1DueDate = _startDate.AddDays(10);

            _collectionPeriods = new List<Domain.ValueObjects.CollectionPeriod>()
            {
                new Domain.ValueObjects.CollectionPeriod(
                    1,
                    (byte)DateTime.Now.Month,
                    (short)DateTime.Now.Year,
                    DateTime.Now.AddDays(-1),
                    DateTime.Now,
                    (short)DateTime.Now.Year,
                    true)
            };

            _mockCollectionCalendarService
                .Setup(m => m.Get())
                .ReturnsAsync(new Domain.ValueObjects.CollectionCalendar(_collectionPeriods));

            _vrfVendorId = Guid.NewGuid().ToString();
            var legalEntity = _fixture.Build<LegalEntityModel>()
                .With(l => l.VrfVendorId, _vrfVendorId)
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

            var model = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(m => m.Account, _account)
                .With(m => m.PlannedStartDate, _startDate)
                .With(m => m.PendingPaymentModels, pendingPayments)
                .Create();

            var incentive = new ApprenticeshipIncentiveFactory().GetExisting(model.Id, model);
            
            _fixture.Register(() => incentive);

            _mockIncentiveDomainRespository
                .Setup(m => m.Find(incentive.Id))
                .ReturnsAsync(incentive);

            _mockAccountDomainRepository
                .Setup(m => m.Find(incentive.Account.Id))
                .ReturnsAsync(domainAccount);

            var submissionData = new SubmissionData(DateTime.UtcNow);
            submissionData.SetLearningFound(new LearningFoundStatus(true));
            submissionData.SetIsInLearning(true);

            _daysInLearning = new DaysInLearning(1, (short)DateTime.Now.Year, 90);

            _learnerModel = _fixture.Build<LearnerModel>()
                .With(m => m.ApprenticeshipId, incentive.Apprenticeship.Id)
                .With(m => m.ApprenticeshipIncentiveId, incentive.Id)
                .With(m => m.UniqueLearnerNumber, incentive.Apprenticeship.UniqueLearnerNumber)
                .With(m => m.SubmissionData, submissionData)
                .With(m => m.DaysInLearnings, new List<DaysInLearning>() { _daysInLearning })
                .Create();

            _learner = new LearnerFactory().GetExisting(_learnerModel);

            _mockLearnerDomainRepository
                .Setup(m => m.GetOrCreate(incentive))
                .ReturnsAsync(_learner);

            _sut = new ValidatePendingPaymentCommandHandler(
                _mockIncentiveDomainRespository.Object,
                _mockAccountDomainRepository.Object,
                _mockCollectionCalendarService.Object,
                _mockLearnerDomainRepository.Object);
        }

        [Test]
        public async Task Then_a_passing_pendingPayment_validation_result_is_saved_against_the_pendingPayment_and_the_pending_payment_is_validated()
        {
            //Arrange
            var incentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();

            var pendingPayment = incentive.PendingPayments.First();
            var collectionPeriod = _collectionPeriods.First();

            var command = new ValidatePendingPaymentCommand(incentive.Id, pendingPayment.Id, collectionPeriod.AcademicYear, collectionPeriod.PeriodNumber);

            // Act
            await _sut.Handle(command);

            // Assert
            incentive.PendingPayments.Count(p => p.PendingPaymentValidationResults.Count >= 1).Should().Be(1);
            incentive.PendingPayments.First().IsValidated(collectionPeriod.AcademicYear, collectionPeriod.PeriodNumber).Should().BeTrue();
        }

        [Test]
        public void Then_a_pendingPayment_is_not_valid_if_no_validations_have_been_run()
        {
            //Arrange
            var collectionPeriod = _collectionPeriods.First();
            var incentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();

            // Assert
            incentive.PendingPayments.Count(p => p.PendingPaymentValidationResults.Count >= 1).Should().Be(0);
            incentive.PendingPayments.First().IsValidated(collectionPeriod.AcademicYear, collectionPeriod.PeriodNumber).Should().BeFalse();
        }

        [Test]
        public async Task Then_a_failed_pendingPayment_validation_result_is_saved_against_the_pendingPayment_and_the_pending_payment_is_not_validated()
        {
            //Arrange
            var incentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();

            var pendingPayment = incentive.PendingPayments.First();
            var collectionPeriod = _collectionPeriods.First();

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
            incentive.PendingPayments.First().IsValidated(collectionPeriod.AcademicYear, collectionPeriod.PeriodNumber).Should().BeFalse(); 
        }

        [Test]
        public async Task Then_a_previously_failed_pendingPayment_validation_result_is_updated_to_passing()
        {
            //Arrange
            var incentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();

            var pendingPayment = incentive.PendingPayments.First();
            var collectionPeriod = _collectionPeriods.First();

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
            incentive.PendingPayments.First().IsValidated(collectionPeriod.AcademicYear, collectionPeriod.PeriodNumber).Should().BeFalse();

            // Act
            accountModel = _fixture.Build<AccountModel>()
                .With(a => a.Id, _account.Id)
                .With(a => a.LegalEntityModels, new List<LegalEntityModel>() {
                   _fixture.Build<LegalEntityModel>()
                   .With(l => l.VrfVendorId, Guid.NewGuid().ToString())
                   .With(l => l.AccountLegalEntityId, _account.AccountLegalEntityId)
                   .Create()})
                .Create();

            domainAccount = Domain.Accounts.Account.Create(accountModel);

            _mockAccountDomainRepository
               .Setup(m => m.Find(incentive.Account.Id))
               .ReturnsAsync(domainAccount);

            await _sut.Handle(command);

            // Assert
            incentive.PendingPayments.Count(p => p.PendingPaymentValidationResults.Count >= 1).Should().Be(1);
            incentive.PendingPayments.First().IsValidated(collectionPeriod.AcademicYear, collectionPeriod.PeriodNumber).Should().BeTrue();
        }


        [Test]
        public async Task Then_a_failed_pendingPayment_validation_result_is_saved_and_no_further_validation_performed_when_no_ILR_submission_found()
        {
            // Arrange
            var incentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();
            _learner.SetSubmissionData(null);
            Assert.IsFalse(_learner.SubmissionFound);

            var pendingPayment = incentive.PendingPayments.First();
            var collectionPeriod = _collectionPeriods.First();

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
            var validationResult = incentive.PendingPayments.Single(x => x.PendingPaymentValidationResults.Count == 2)
                .PendingPaymentValidationResults.Single(x => x.Step == "HasIlrSubmission");
            validationResult.Result.Should().BeFalse();
        }
    }
}
