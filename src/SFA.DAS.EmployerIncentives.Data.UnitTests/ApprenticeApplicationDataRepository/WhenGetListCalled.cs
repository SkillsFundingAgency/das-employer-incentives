using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AcademicYear = SFA.DAS.EmployerIncentives.Domain.ValueObjects.AcademicYear;
using ClawbackPayment = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.ClawbackPayment;
using Payment = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.Payment;
using PendingPayment = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.PendingPayment;
using PendingPaymentValidationResult = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.PendingPaymentValidationResult;
using ValidationOverride = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.ValidationOverride;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.ApprenticeApplicationDataRepository
{
    [TestFixture]
    public class WhenGetListCalled
    {
        private EmployerIncentivesDbContext _context;
        private Fixture _fixture;
        private Data.ApprenticeApplicationDataRepository _sut;
        private DbContextOptions<EmployerIncentivesDbContext> _options;
        private Mock<IDateTimeService> _mockDateTimeService;
        private Mock<ICollectionCalendarService> _mockCollectionCalendarService;
        private List<Domain.ValueObjects.CollectionCalendarPeriod> _collectionPeriods;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _context = new EmployerIncentivesDbContext(_options);

            _mockDateTimeService = new Mock<IDateTimeService>();
            _mockCollectionCalendarService = new Mock<ICollectionCalendarService>();


            _collectionPeriods = new List<Domain.ValueObjects.CollectionCalendarPeriod>()
            {
                new Domain.ValueObjects.CollectionCalendarPeriod(new Domain.ValueObjects.CollectionPeriod(1, (short)DateTime.Now.Year), (byte)DateTime.Now.Month, (short)DateTime.Now.Year, DateTime.Now.AddDays(-1), DateTime.Now, true, false),
                new Domain.ValueObjects.CollectionCalendarPeriod(new Domain.ValueObjects.CollectionPeriod(2, (short)DateTime.Now.AddMonths(1).Year), (byte)DateTime.Now.AddMonths(1).Month, (short)DateTime.Now.AddMonths(1).Year, DateTime.Now.AddMonths(1).AddDays(-1), DateTime.Now.AddMonths(1), false, false)
            };

            _mockCollectionCalendarService
                .Setup(m => m.Get())
                .ReturnsAsync(new Domain.ValueObjects.CollectionCalendar(new List<AcademicYear>(), _collectionPeriods));

            _sut = new Data.ApprenticeApplicationDataRepository(new Lazy<EmployerIncentivesDbContext>(_context), _mockDateTimeService.Object, _mockCollectionCalendarService.Object);
        }

        [TearDown]
        public void CleanUp()
        {
            _context.Dispose();
        }

        [Test]
        public async Task Then_data_is_fetched_from_the_database()
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>(5).ToArray();
            incentives[0].AccountId = accountId;
            incentives[0].AccountLegalEntityId = accountLegalEntityId;
            incentives[0].PausePayments = false;
            incentives[0].MinimumAgreementVersion = allAccounts[0].SignedAgreementVersion;

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.ClawedBack, false)
                .With(p => p.ValidationResults, new List<PendingPaymentValidationResult>())
                .CreateMany(2).ToList();
            pendingPayments[0].DueDate = DateTime.Parse("04-01-2020", new CultureInfo("en-GB"));
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].DueDate = DateTime.Parse("01-12-2020", new CultureInfo("en-GB"));
            pendingPayments[1].EarningType = EarningType.SecondPayment;

            incentives[0].PendingPayments = pendingPayments;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            result.Length.Should().Be(1);
            result.All(x => x.AccountId == accountId).Should().BeTrue();

            var expectedFirstPaymentStatus = new PaymentStatusDto
            {
                LearnerMatchFound = false,
                PaymentSentIsEstimated = true,
                PaymentAmount = pendingPayments[0].Amount,
                PaymentDate = new DateTime(DateTime.Now.AddMonths(1).Year, DateTime.Now.AddMonths(1).Month, 4),
                RequiresNewEmployerAgreement = false
            };

            var expectedSecondPaymentStatus = new PaymentStatusDto
            {
                LearnerMatchFound = false,
                PaymentAmount = pendingPayments[1].Amount,
                PaymentDate = new DateTime(DateTime.Now.AddMonths(1).Year, DateTime.Now.AddMonths(1).Month, 27),
                PaymentSentIsEstimated = true,  // update when implementing EI-827
                RequiresNewEmployerAgreement = false
            };
            result[0].SecondPaymentStatus.Should().BeEquivalentTo(expectedSecondPaymentStatus);
        }

        [Test]
        public async Task Then_learner_information_is_populated_to_indicate_no_learner_record_found()
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>(5).ToArray();
            incentives[0].AccountId = accountId;
            incentives[0].AccountLegalEntityId = accountLegalEntityId;
            incentives[0].PausePayments = false;

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.ClawedBack, false)
                .CreateMany(2).ToList();
            pendingPayments[0].DueDate = DateTime.Parse("04-01-2020", new CultureInfo("en-GB"));
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].DueDate = DateTime.Parse("01-12-2020", new CultureInfo("en-GB"));
            pendingPayments[1].EarningType = EarningType.SecondPayment;

            incentives[0].PendingPayments = pendingPayments;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            foreach (var application in result)
            {
                application.FirstPaymentStatus.LearnerMatchFound.Should().BeFalse();
                application.SecondPaymentStatus.LearnerMatchFound.Should().BeFalse();
                application.FirstPaymentStatus.HasDataLock.Should().BeFalse();
                application.SecondPaymentStatus.HasDataLock.Should().BeFalse();
                application.FirstPaymentStatus.InLearning.Should().BeFalse();
                application.SecondPaymentStatus.InLearning.Should().BeFalse();
                application.FirstPaymentStatus.PausePayments.Should().BeFalse();
                application.SecondPaymentStatus.PausePayments.Should().BeFalse();
            }
        }

        [Test]
        public async Task Then_learner_match_found_is_false_if_learner_record_and_no_match()
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>(5).ToArray();
            incentives[0].AccountId = accountId;
            incentives[0].AccountLegalEntityId = accountLegalEntityId;

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.ClawedBack, false)
                .CreateMany(2).ToList();
            pendingPayments[0].DueDate = DateTime.Parse("04-01-2020", new CultureInfo("en-GB"));
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].DueDate = DateTime.Parse("01-12-2020", new CultureInfo("en-GB"));
            pendingPayments[1].EarningType = EarningType.SecondPayment;

            incentives[0].PendingPayments = pendingPayments;

            var learners = _fixture.CreateMany<ApprenticeshipIncentives.Models.Learner>(10).ToList();
            learners[0].ULN = incentives[0].ULN;
            learners[0].LearningFound = false;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);
            _context.Learners.AddRange(learners);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            var application = result.FirstOrDefault(x => x.ULN == incentives[0].ULN);
            application.FirstPaymentStatus.LearnerMatchFound.Should().BeFalse();
            application.SecondPaymentStatus.LearnerMatchFound.Should().BeFalse();
        }

        [Test]
        public async Task Then_learner_match_found_is_true_if_learner_record_with_match()
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>(5).ToArray();
            incentives[0].AccountId = accountId;
            incentives[0].AccountLegalEntityId = accountLegalEntityId;

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.ClawedBack, false)
                .CreateMany(2).ToList();
            pendingPayments[0].DueDate = DateTime.Parse("04-01-2020", new CultureInfo("en-GB"));
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].DueDate = DateTime.Parse("01-12-2020", new CultureInfo("en-GB"));
            pendingPayments[1].EarningType = EarningType.SecondPayment;

            incentives[0].PendingPayments = pendingPayments;

            var learners = _fixture.CreateMany<ApprenticeshipIncentives.Models.Learner>(10).ToList();
            learners[0].ULN = incentives[0].ULN;
            learners[0].ApprenticeshipIncentiveId = incentives[0].Id;
            learners[0].LearningFound = true;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);
            _context.Learners.AddRange(learners);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            var application = result.FirstOrDefault(x => x.ULN == incentives[0].ULN);
            application.FirstPaymentStatus.LearnerMatchFound.Should().BeTrue();
            application.SecondPaymentStatus.LearnerMatchFound.Should().BeTrue();
        }

        [Test]
        public async Task Then_has_data_lock_is_true_if_populated_in_learner_record()
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>(5).ToArray();
            incentives[0].AccountId = accountId;
            incentives[0].AccountLegalEntityId = accountLegalEntityId;

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.ClawedBack, false)
                .CreateMany(2).ToList();
            pendingPayments[0].DueDate = DateTime.Parse("04-01-2020", new CultureInfo("en-GB"));
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].DueDate = DateTime.Parse("01-12-2020", new CultureInfo("en-GB"));
            pendingPayments[1].EarningType = EarningType.SecondPayment;

            incentives[0].PendingPayments = pendingPayments;

            var learners = _fixture.CreateMany<ApprenticeshipIncentives.Models.Learner>(10).ToList();
            learners[0].ULN = incentives[0].ULN;
            learners[0].ApprenticeshipIncentiveId = incentives[0].Id;
            learners[0].HasDataLock = true;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);
            _context.Learners.AddRange(learners);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            var application = result.FirstOrDefault(x => x.ULN == incentives[0].ULN);
            application.FirstPaymentStatus.HasDataLock.Should().BeTrue();
            application.SecondPaymentStatus.HasDataLock.Should().BeTrue();
        }

        [Test]
        public async Task Then_apprentice_in_learning_is_false_if_populated_in_learner_record()
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>(5).ToArray();
            incentives[0].AccountId = accountId;
            incentives[0].AccountLegalEntityId = accountLegalEntityId;

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.ClawedBack, false)
                .CreateMany(2).ToList();
            pendingPayments[0].DueDate = DateTime.Parse("04-01-2020", new CultureInfo("en-GB"));
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].DueDate = DateTime.Parse("01-12-2020", new CultureInfo("en-GB"));
            pendingPayments[1].EarningType = EarningType.SecondPayment;

            incentives[0].PendingPayments = pendingPayments;

            var learners = _fixture.CreateMany<ApprenticeshipIncentives.Models.Learner>(10).ToList();
            learners[0].ULN = incentives[0].ULN;
            learners[0].ApprenticeshipIncentiveId = incentives[0].Id;
            learners[0].InLearning = false;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);
            _context.Learners.AddRange(learners);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            var application = result.FirstOrDefault(x => x.ULN == incentives[0].ULN);
            application.FirstPaymentStatus.InLearning.Should().BeFalse();
        }

        [Test]
        public async Task Then_payments_paused_is_true_if_populated_in_learner_record()
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>(5).ToArray();
            incentives[0].AccountId = accountId;
            incentives[0].AccountLegalEntityId = accountLegalEntityId;

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.ClawedBack, false)
                .CreateMany(2).ToList();
            pendingPayments[0].DueDate = DateTime.Parse("04-01-2020", new CultureInfo("en-GB"));
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].DueDate = DateTime.Parse("01-12-2020", new CultureInfo("en-GB"));
            pendingPayments[1].EarningType = EarningType.SecondPayment;

            incentives[0].PendingPayments = pendingPayments;
            incentives[0].PausePayments = true;

            var learners = _fixture.CreateMany<ApprenticeshipIncentives.Models.Learner>(10).ToList();
            learners[0].ULN = incentives[0].ULN;
            learners[0].ApprenticeshipIncentiveId = incentives[0].Id;
            learners[0].InLearning = false;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);
            _context.Learners.AddRange(learners);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            var application = result.FirstOrDefault(x => x.ULN == incentives[0].ULN);
            application.FirstPaymentStatus.PausePayments.Should().BeTrue();
            application.SecondPaymentStatus.PausePayments.Should().BeTrue();
        }

        [Test]
        public async Task Then_first_payment_date_is_payment_paid_date_when_payment_made()
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>(5).ToArray();
            incentives[0].AccountId = accountId;
            incentives[0].AccountLegalEntityId = accountLegalEntityId;

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.ClawedBack, false)
                .CreateMany(2).ToList();
            pendingPayments[0].DueDate = DateTime.Parse("04-01-2020", new CultureInfo("en-GB"));
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].DueDate = DateTime.Parse("01-12-2020", new CultureInfo("en-GB"));
            pendingPayments[1].EarningType = EarningType.SecondPayment;

            var payments = _fixture
                .Build<Payment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.PendingPaymentId, pendingPayments[0].Id)
                .CreateMany(2).ToList();

            payments[0].PaidDate = pendingPayments[0].DueDate.AddDays(1);

            incentives[0].PendingPayments = pendingPayments;
            incentives[0].Payments = payments;

            var learners = _fixture.CreateMany<ApprenticeshipIncentives.Models.Learner>(10).ToList();
            learners[0].ULN = incentives[0].ULN;
            learners[0].ApprenticeshipIncentiveId = incentives[0].Id;
            learners[0].InLearning = false;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);
            _context.Learners.AddRange(learners);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            var application = result.FirstOrDefault(x => x.ULN == incentives[0].ULN);
            application.FirstPaymentStatus.PaymentDate.Should().Be(payments[0].PaidDate);
        }

        [Test]
        public async Task Then_first_payment_date_is_payment_calculated_date_when_payment_exists_but_not_made()
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>(5).ToArray();
            incentives[0].AccountId = accountId;
            incentives[0].AccountLegalEntityId = accountLegalEntityId;

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.ClawedBack, false)
                .CreateMany(2).ToList();
            pendingPayments[0].DueDate = DateTime.Parse("04-01-2020", new CultureInfo("en-GB"));
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].DueDate = DateTime.Parse("01-12-2020", new CultureInfo("en-GB"));
            pendingPayments[1].EarningType = EarningType.SecondPayment;

            var payments = _fixture
                .Build<Payment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.PendingPaymentId, pendingPayments[0].Id)
                .CreateMany(2).ToList();

            payments[0].CalculatedDate = new DateTime(pendingPayments[0].DueDate.Year, pendingPayments[0].DueDate.Month, 26);
            payments[0].PaidDate = null;

            incentives[0].PendingPayments = pendingPayments;
            incentives[0].Payments = payments;

            var learners = _fixture.CreateMany<ApprenticeshipIncentives.Models.Learner>(10).ToList();
            learners[0].ULN = incentives[0].ULN;
            learners[0].ApprenticeshipIncentiveId = incentives[0].Id;
            learners[0].InLearning = false;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);
            _context.Learners.AddRange(learners);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            var application = result.FirstOrDefault(x => x.ULN == incentives[0].ULN);
            application.FirstPaymentStatus.PaymentDate.Should().Be(payments[0].CalculatedDate);
        }

        [Test]
        public async Task Then_first_payment_date_is_month_after_pending_payment_due_date_when_payment_does_not_exist()
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>(5).ToArray();
            incentives[0].AccountId = accountId;
            incentives[0].AccountLegalEntityId = accountLegalEntityId;

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.ClawedBack, false)
                .CreateMany(2).ToList();

            pendingPayments[0].DueDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 4);
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].DueDate = DateTime.Parse("01-12-2020", new CultureInfo("en-GB"));
            pendingPayments[1].EarningType = EarningType.SecondPayment;

            incentives[0].PendingPayments = pendingPayments;

            var learners = _fixture.CreateMany<ApprenticeshipIncentives.Models.Learner>(10).ToList();
            learners[0].ULN = incentives[0].ULN;
            learners[0].ApprenticeshipIncentiveId = incentives[0].Id;
            learners[0].InLearning = false;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);
            _context.Learners.AddRange(learners);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            var application = result.FirstOrDefault(x => x.ULN == incentives[0].ULN);
            application.FirstPaymentStatus.PaymentDate.Should().Be(new DateTime(pendingPayments[0].DueDate.AddMonths(1).Year, pendingPayments[0].DueDate.AddMonths(1).Month, 27));
        }

        [Test]
        public async Task Then_first_payment_date_is_the_next_active_period_when_payment_does_not_exist_and_pending_payment_due_date_is_in_current_period()
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>(5).ToArray();
            incentives[0].AccountId = accountId;
            incentives[0].AccountLegalEntityId = accountLegalEntityId;

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.ClawedBack, false)
                .CreateMany(2).ToList();

            pendingPayments[0].DueDate = _collectionPeriods.Single(p => p.CollectionPeriod.PeriodNumber == 1).OpenDate.AddDays(-1);
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].DueDate = DateTime.Parse("01-12-2020", new CultureInfo("en-GB"));
            pendingPayments[1].EarningType = EarningType.SecondPayment;

            incentives[0].PendingPayments = pendingPayments;

            var learners = _fixture.CreateMany<ApprenticeshipIncentives.Models.Learner>(10).ToList();
            learners[0].ULN = incentives[0].ULN;
            learners[0].ApprenticeshipIncentiveId = incentives[0].Id;
            learners[0].InLearning = false;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);
            _context.Learners.AddRange(learners);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            var application = result.FirstOrDefault(x => x.ULN == incentives[0].ULN);
            application.FirstPaymentStatus.PaymentDate.Value.Year.Should().Be(_collectionPeriods.Single(p => p.CollectionPeriod.PeriodNumber == 2).CollectionPeriod.AcademicYear);
            application.FirstPaymentStatus.PaymentDate.Value.Month.Should().Be(_collectionPeriods.Single(p => p.CollectionPeriod.PeriodNumber == 2).CalendarMonth);
            application.FirstPaymentStatus.PaymentDate.Value.Day.Should().Be(27);
        }

        [Test]
        public async Task Then_first_payment_amount_is_the_payment_amount()
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>(5).ToArray();
            incentives[0].AccountId = accountId;
            incentives[0].AccountLegalEntityId = accountLegalEntityId;

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.ClawedBack, false)
                .CreateMany(2).ToList();
            pendingPayments[0].DueDate = DateTime.Parse("04-01-2020", new CultureInfo("en-GB"));
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].DueDate = DateTime.Parse("01-12-2020", new CultureInfo("en-GB"));
            pendingPayments[1].EarningType = EarningType.SecondPayment;

            var payments = _fixture
                .Build<Payment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.PendingPaymentId, pendingPayments[0].Id)
                .CreateMany(2).ToList();

            incentives[0].PendingPayments = pendingPayments;
            incentives[0].Payments = payments;

            var learners = _fixture.CreateMany<ApprenticeshipIncentives.Models.Learner>(10).ToList();
            learners[0].ULN = incentives[0].ULN;
            learners[0].ApprenticeshipIncentiveId = incentives[0].Id;
            learners[0].InLearning = false;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);
            _context.Learners.AddRange(learners);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            var application = result.FirstOrDefault(x => x.ULN == incentives[0].ULN);
            application.FirstPaymentStatus.PaymentAmount.Should().Be(payments[0].Amount);
        }

        [Test]
        public async Task Then_first_payment_amount_is_the_pending_payment_amount_when_the_payment_does_not_exist()
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>(5).ToArray();
            incentives[0].AccountId = accountId;
            incentives[0].AccountLegalEntityId = accountLegalEntityId;

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.ClawedBack, false)
                .CreateMany(2).ToList();
            pendingPayments[0].DueDate = DateTime.Parse("04-01-2020", new CultureInfo("en-GB"));
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].DueDate = DateTime.Parse("01-12-2020", new CultureInfo("en-GB"));
            pendingPayments[1].EarningType = EarningType.SecondPayment;


            incentives[0].PendingPayments = pendingPayments;

            var learners = _fixture.CreateMany<ApprenticeshipIncentives.Models.Learner>(10).ToList();
            learners[0].ULN = incentives[0].ULN;
            learners[0].ApprenticeshipIncentiveId = incentives[0].Id;
            learners[0].InLearning = false;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);
            _context.Learners.AddRange(learners);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            var application = result.FirstOrDefault(x => x.ULN == incentives[0].ULN);
            application.FirstPaymentStatus.PaymentAmount.Should().Be(pendingPayments[0].Amount);
        }

        [Test]
        public async Task Then_first_payment_is_estimated_when_the_payment_does_not_exist()
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>(5).ToArray();
            incentives[0].AccountId = accountId;
            incentives[0].AccountLegalEntityId = accountLegalEntityId;

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.ClawedBack, false)
                .CreateMany(2).ToList();
            pendingPayments[0].DueDate = DateTime.Parse("04-01-2020", new CultureInfo("en-GB"));
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].DueDate = DateTime.Parse("01-12-2020", new CultureInfo("en-GB"));
            pendingPayments[1].EarningType = EarningType.SecondPayment;

            incentives[0].PendingPayments = pendingPayments;

            var learners = _fixture.CreateMany<ApprenticeshipIncentives.Models.Learner>(10).ToList();
            learners[0].ULN = incentives[0].ULN;
            learners[0].ApprenticeshipIncentiveId = incentives[0].Id;
            learners[0].InLearning = false;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);
            _context.Learners.AddRange(learners);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            var application = result.FirstOrDefault(x => x.ULN == incentives[0].ULN);
            application.FirstPaymentStatus.PaymentSentIsEstimated.Should().BeTrue();
        }

        [Test()]
        public async Task Then_first_payment_is_estimated_when_the_payment_has_not_been_made()
        {
            for (int i = 1; i < 31; i++)
            {
                await Then_first_payment_is_as_expected_for_the_payment_and_the_calculated_date_is_set(i, false, true);
            }
            Assert.Pass();
        }

        [Test()]
        public async Task Then_first_payment_is_not_estimated_when_the_payment_has_been_made_and_the_current_date_is_day_27_or_greater()
        {
            for (int i = 27; i < 31; i++)
            {
                await Then_first_payment_is_as_expected_for_the_payment_and_the_calculated_date_is_set(i, true, false);
            }
            Assert.Pass();
        }

        [Test()]
        public async Task Then_first_payment_is_estimated_when_the_payment_has_been_made_and_the_current_date_is_less_than_day_27()
        {
            for (int i = 1; i < 27; i++)
            {
                await Then_first_payment_is_as_expected_for_the_payment_and_the_calculated_date_is_set(i, true, true);
            }
            Assert.Pass();
        }
        private async Task Then_first_payment_is_as_expected_for_the_payment_and_the_calculated_date_is_set(
            int day,
            bool paymentMade,
            bool expected)
        {
            // Arrange            
            _context = new EmployerIncentivesDbContext(_options);
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>(5).ToArray();
            incentives[0].AccountId = accountId;
            incentives[0].AccountLegalEntityId = accountLegalEntityId;

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.ClawedBack, false)
                .CreateMany(2).ToList();

            pendingPayments[0].DueDate = DateTime.Parse("04-01-2020", new CultureInfo("en-GB"));
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].DueDate = DateTime.Parse("01-12-2020", new CultureInfo("en-GB"));
            pendingPayments[1].EarningType = EarningType.SecondPayment;

            var payments = _fixture
                .Build<Payment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.PendingPaymentId, pendingPayments[0].Id)
                .CreateMany(2).ToList();

            _mockDateTimeService.Setup(m => m.Now()).Returns(new DateTime(pendingPayments[0].DueDate.Year, 1, day));
            if (!paymentMade)
            {
                payments[0].PaidDate = null;
            }
            else
            {
                payments[0].PaidDate = new DateTime(pendingPayments[0].DueDate.Year, 1, payments[0].PaidDate.Value.Day);
            }

            incentives[0].PendingPayments = pendingPayments;
            incentives[0].Payments = payments;

            var learners = _fixture.CreateMany<ApprenticeshipIncentives.Models.Learner>(10).ToList();
            learners[0].ULN = incentives[0].ULN;
            learners[0].ApprenticeshipIncentiveId = incentives[0].Id;
            learners[0].InLearning = false;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);
            _context.Learners.AddRange(learners);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            var application = result.FirstOrDefault(x => x.ULN == incentives[0].ULN);
            application.FirstPaymentStatus.PaymentSentIsEstimated.Should().Be(expected);
        }

        [Test]
        public async Task Then_first_payment_is_not_sent_if_the_payment_record_has_not_been_created()
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>(5).ToArray();
            incentives[0].AccountId = accountId;
            incentives[0].AccountLegalEntityId = accountLegalEntityId;

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.ClawedBack, false)
                .CreateMany(2).ToList();
            pendingPayments[0].DueDate = DateTime.Parse("04-01-2020", new CultureInfo("en-GB"));
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].DueDate = DateTime.Parse("01-12-2020", new CultureInfo("en-GB"));
            pendingPayments[1].EarningType = EarningType.SecondPayment;

            incentives[0].PendingPayments = pendingPayments;

            var learners = _fixture.CreateMany<ApprenticeshipIncentives.Models.Learner>(10).ToList();
            learners[0].ULN = incentives[0].ULN;
            learners[0].ApprenticeshipIncentiveId = incentives[0].Id;
            learners[0].InLearning = false;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);
            _context.Learners.AddRange(learners);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            var application = result.FirstOrDefault(x => x.ULN == incentives[0].ULN);
            application.FirstPaymentStatus.PaymentSent.Should().BeFalse();
        }

        [TestCase(null, null, true)]
        [TestCase(null, 1, true)]
        [TestCase(1, null, false)]
        [TestCase(1, 1, false)]
        [TestCase(1, 2, true)]
        [TestCase(2, 1, false)]
        public async Task Then_requires_new_employer_agreement_is_set(int? accountVersion, int? incentiveVersion, bool requiresNewVersion)
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(1).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;
            allAccounts[0].SignedAgreementVersion = accountVersion;

            var incentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>(1).ToArray();
            incentives[0].AccountId = accountId;
            incentives[0].AccountLegalEntityId = accountLegalEntityId;
            incentives[0].MinimumAgreementVersion = incentiveVersion;

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.ClawedBack, false)
                .CreateMany(2).ToList();
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].EarningType = EarningType.SecondPayment;

            incentives[0].PendingPayments = pendingPayments;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            result[0].FirstPaymentStatus.RequiresNewEmployerAgreement.Should().Be(requiresNewVersion);
            result[0].SecondPaymentStatus.RequiresNewEmployerAgreement.Should().Be(requiresNewVersion);
        }

        [TestCase(false, false, false, null, null)]
        [TestCase(false, true, false, null, null)]
        [TestCase(false, true, true, null, null)]
        [TestCase(true, false, false, true, null)]
        [TestCase(true, true, false, false, true)]
        [TestCase(true, true, true, false, false)]
        public async Task Then_payment_isStopped_is_set(
            bool apprenticehipStopped,
            bool hasFirstPaymentStatus,
            bool hasSecondPaymentStatus,
            bool? firstPaymentStatusPaymentIsStopped,
            bool? secondPaymentStatusPaymentIsStopped)
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(1).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentive = _fixture.Build<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.Status, apprenticehipStopped ? IncentiveStatus.Stopped : IncentiveStatus.Active)
                .Create();

            var pendingPayments = new List<PendingPayment>();

            if (hasFirstPaymentStatus)
            {
                pendingPayments.Add(
                    _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentive.Id)
                .With(p => p.EarningType, EarningType.FirstPayment)
                .With(p => p.ClawedBack, false)
                .Create());
            }

            if (hasSecondPaymentStatus)
            {
                pendingPayments.Add(
                    _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentive.Id)
                .With(p => p.EarningType, EarningType.SecondPayment)
                .With(p => p.ClawedBack, false)
                .Create());
            }

            incentive.PendingPayments = pendingPayments;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentive);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            if (firstPaymentStatusPaymentIsStopped != null)
            {
                result[0].FirstPaymentStatus.PaymentIsStopped.Should().Be(firstPaymentStatusPaymentIsStopped.Value);
            }
            if (secondPaymentStatusPaymentIsStopped != null)
            {
                result[0].SecondPaymentStatus.PaymentIsStopped.Should().Be(secondPaymentStatusPaymentIsStopped.Value);
            }
        }

        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public async Task Then_withdrawn_by_employer_is_set(
            bool hasFirstPayment,
            bool withdrawnByEmployer)
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(1).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentive = _fixture.Build<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.Status, IncentiveStatus.Withdrawn)
                .With(p => p.WithdrawnBy, withdrawnByEmployer ? WithdrawnBy.Employer : WithdrawnBy.Compliance)
                .Create();

            var pendingPayments = new List<PendingPayment>();

            if (hasFirstPayment)
            {
                pendingPayments.Add(
                    _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentive.Id)
                .With(p => p.EarningType, EarningType.FirstPayment)
                .With(p => p.ClawedBack, false)
                .Create());
            }

            incentive.PendingPayments = pendingPayments;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentive);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            if (!hasFirstPayment)
            {
                result[0].FirstPaymentStatus.WithdrawnByEmployer.Should().Be(withdrawnByEmployer);
            }
            else
            {
                result[0].SecondPaymentStatus.WithdrawnByEmployer.Should().Be(withdrawnByEmployer);
            }
        }

        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public async Task Then_withdrawn_by_compliance_is_set(
            bool hasFirstPayment,
            bool withdrawnByCompliance)
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(1).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentive = _fixture.Build<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.Status, IncentiveStatus.Withdrawn)
                .With(p => p.WithdrawnBy, withdrawnByCompliance ? WithdrawnBy.Compliance : WithdrawnBy.Employer)
                .Create();

            var pendingPayments = new List<PendingPayment>();

            if (hasFirstPayment)
            {
                pendingPayments.Add(
                    _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentive.Id)
                .With(p => p.EarningType, EarningType.FirstPayment)
                .With(p => p.ClawedBack, false)
                .Create());
            }

            incentive.PendingPayments = pendingPayments;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentive);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            if (!hasFirstPayment)
            {
                result[0].FirstPaymentStatus.WithdrawnByCompliance.Should().Be(withdrawnByCompliance);
            }
            else
            {
                result[0].SecondPaymentStatus.WithdrawnByCompliance.Should().Be(withdrawnByCompliance);
            }
        }

        [Test]
        public async Task Then_clawback_status_is_populated_if_first_payment_clawed_back()
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(1).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentive = _fixture.Build<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.Status, IncentiveStatus.Active)
                .Create();

            var apprenticeship = _fixture.Build<Models.IncentiveApplicationApprenticeship>()
                .With(p => p.IncentiveApplicationId, incentive.Id)
                .Create();

            var pendingPayments = new List<PendingPayment>
            {
                _fixture
                    .Build<PendingPayment>()
                    .With(p => p.AccountId, accountId)
                    .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                    .With(p => p.ApprenticeshipIncentiveId, incentive.Id)
                    .With(p => p.EarningType, EarningType.FirstPayment)
                    .With(p => p.ClawedBack, true)
                    .With(p => p.ValidationResults, new List<PendingPaymentValidationResult>())
                    .Create(),
                _fixture
                    .Build<PendingPayment>()
                    .With(p => p.AccountId, accountId)
                    .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                    .With(p => p.ApprenticeshipIncentiveId, incentive.Id)
                    .With(p => p.EarningType, EarningType.FirstPayment)
                    .With(p => p.ClawedBack, false)
                    .With(p => p.ValidationResults, new List<PendingPaymentValidationResult>())
                    .Create(),
                _fixture
                    .Build<PendingPayment>()
                    .With(p => p.AccountId, accountId)
                    .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                    .With(p => p.ApprenticeshipIncentiveId, incentive.Id)
                    .With(p => p.EarningType, EarningType.SecondPayment)
                    .With(p => p.ClawedBack, false)
                    .With(p => p.ValidationResults, new List<PendingPaymentValidationResult>())
                    .Create()
            };

            var payment = _fixture
                .Build<Payment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentive.Id)
                .With(p => p.PendingPaymentId, pendingPayments[0].Id)
                .With(p => p.PaidDate, new DateTime(2021, 1, 1))
                .Create();
            payment.PaidDate = new DateTime(pendingPayments[0].DueDate.Year, 1, payment.PaidDate.Value.Day);

            var clawback = _fixture.Build<ClawbackPayment>()
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentive.Id)
                .With(p => p.AccountId, accountId)
                .With(p => p.PaymentId, payment.Id)
                .With(p => p.PendingPaymentId, pendingPayments[0].Id)
                .Create();

            incentive.PendingPayments = pendingPayments;
            incentive.Payments = new List<Payment> { payment };

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentive);
            _context.ApplicationApprenticeships.AddRange(apprenticeship);
            _context.ClawbackPayments.AddRange(clawback);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            result.Length.Should().Be(1);
            result[0].FirstClawbackStatus.Should().NotBeNull();
            result[0].FirstClawbackStatus.ClawbackAmount.Should().Be(clawback.Amount);
            result[0].FirstClawbackStatus.ClawbackDate.Should().Be(clawback.DateClawbackCreated);
            result[0].FirstClawbackStatus.OriginalPaymentDate.Should().Be(payment.PaidDate.Value);
        }

        [Test]
        public async Task Then_clawback_status_is_populated_if_second_payment_clawed_back()
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(1).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentive = _fixture.Build<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.Status, IncentiveStatus.Active)
                .Create();

            var apprenticeship = _fixture.Build<Models.IncentiveApplicationApprenticeship>()
                .With(p => p.IncentiveApplicationId, incentive.Id)
                .Create();

            var pendingPayments = new List<PendingPayment>
            {
                _fixture
                    .Build<PendingPayment>()
                    .With(p => p.AccountId, accountId)
                    .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                    .With(p => p.ApprenticeshipIncentiveId, incentive.Id)
                    .With(p => p.EarningType, EarningType.FirstPayment)
                    .With(p => p.ClawedBack, false)
                    .With(p => p.ValidationResults, new List<PendingPaymentValidationResult>())
                    .Create(),
                _fixture
                    .Build<PendingPayment>()
                    .With(p => p.AccountId, accountId)
                    .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                    .With(p => p.ApprenticeshipIncentiveId, incentive.Id)
                    .With(p => p.EarningType, EarningType.SecondPayment)
                    .With(p => p.ClawedBack, true)
                    .With(p => p.ValidationResults, new List<PendingPaymentValidationResult>())
                    .Create(),
                _fixture
                    .Build<PendingPayment>()
                    .With(p => p.AccountId, accountId)
                    .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                    .With(p => p.ApprenticeshipIncentiveId, incentive.Id)
                    .With(p => p.EarningType, EarningType.SecondPayment)
                    .With(p => p.ClawedBack, false)
                    .With(p => p.ValidationResults, new List<PendingPaymentValidationResult>())
                    .Create()
            };

            var payments = _fixture
                .Build<Payment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentive.Id)
                .With(p => p.PendingPaymentId, pendingPayments[1].Id)
                .With(p => p.PaidDate, new DateTime(2021, 1, 1))
                .CreateMany(2).ToList();
            payments[1].PaidDate = new DateTime(pendingPayments[1].DueDate.Year, 1, payments[1].PaidDate.Value.Day);

            var clawback = _fixture.Build<ClawbackPayment>()
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentive.Id)
                .With(p => p.AccountId, accountId)
                .With(p => p.PaymentId, payments[1].Id)
                .With(p => p.PendingPaymentId, pendingPayments[1].Id)
                .Create();

            incentive.PendingPayments = pendingPayments;
            incentive.Payments = payments;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentive);
            _context.ApplicationApprenticeships.AddRange(apprenticeship);
            _context.ClawbackPayments.AddRange(clawback);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            result.Length.Should().Be(1);
            result[0].SecondClawbackStatus.Should().NotBeNull();
            result[0].SecondClawbackStatus.ClawbackAmount.Should().Be(clawback.Amount);
            result[0].SecondClawbackStatus.ClawbackDate.Should().Be(clawback.DateClawbackCreated);
            result[0].SecondClawbackStatus.OriginalPaymentDate.Should().Be(payments[1].PaidDate.Value);
        }

        [Test]
        public async Task Then_clawback_date_is_set_if_first_payment_clawed_back_but_not_sent()
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(1).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentive = _fixture.Build<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.Status, IncentiveStatus.Active)
                .Create();

            var apprenticeship = _fixture.Build<Models.IncentiveApplicationApprenticeship>()
                .With(p => p.IncentiveApplicationId, incentive.Id)
                .Create();

            var pendingPayments = new List<PendingPayment>
            {
                _fixture
                    .Build<PendingPayment>()
                    .With(p => p.AccountId, accountId)
                    .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                    .With(p => p.ApprenticeshipIncentiveId, incentive.Id)
                    .With(p => p.EarningType, EarningType.FirstPayment)
                    .With(p => p.ClawedBack, true)
                    .With(p => p.ValidationResults, new List<PendingPaymentValidationResult>())
                    .Create(),
                _fixture
                    .Build<PendingPayment>()
                    .With(p => p.AccountId, accountId)
                    .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                    .With(p => p.ApprenticeshipIncentiveId, incentive.Id)
                    .With(p => p.EarningType, EarningType.FirstPayment)
                    .With(p => p.ClawedBack, false)
                    .With(p => p.ValidationResults, new List<PendingPaymentValidationResult>())
                    .Create(),
                _fixture
                    .Build<PendingPayment>()
                    .With(p => p.AccountId, accountId)
                    .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                    .With(p => p.ApprenticeshipIncentiveId, incentive.Id)
                    .With(p => p.EarningType, EarningType.SecondPayment)
                    .With(p => p.ClawedBack, false)
                    .With(p => p.ValidationResults, new List<PendingPaymentValidationResult>())
                    .Create()
            };

            var payments = _fixture
                .Build<Payment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentive.Id)
                .With(p => p.PendingPaymentId, pendingPayments[0].Id)
                .With(p => p.PaidDate, new DateTime(2021, 1, 1))
                .CreateMany(2).ToList();
            payments[0].PaidDate = new DateTime(pendingPayments[0].DueDate.Year, 1, payments[0].PaidDate.Value.Day);

            DateTime? nullClawbackDate = null;
            var clawback = _fixture.Build<ClawbackPayment>()
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentive.Id)
                .With(p => p.AccountId, accountId)
                .With(p => p.PaymentId, payments[0].Id)
                .With(p => p.PendingPaymentId, pendingPayments[0].Id)
                .With(p => p.DateClawbackSent, nullClawbackDate)
                .Create();

            incentive.PendingPayments = pendingPayments;
            incentive.Payments = payments;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentive);
            _context.ApplicationApprenticeships.AddRange(apprenticeship);
            _context.ClawbackPayments.AddRange(clawback);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            result.Length.Should().Be(1);
            result[0].FirstClawbackStatus.Should().NotBeNull();
            result[0].FirstClawbackStatus.ClawbackAmount.Should().Be(clawback.Amount);
            result[0].FirstClawbackStatus.ClawbackDate.Should().Be(clawback.DateClawbackCreated);
            result[0].FirstClawbackStatus.OriginalPaymentDate.Should().Be(payments[0].PaidDate.Value);
        }

        [Test]
        public async Task Then_second_payment_is_estimated_when_the_payment_does_not_exist()
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>(5).ToArray();
            incentives[0].AccountId = accountId;
            incentives[0].AccountLegalEntityId = accountLegalEntityId;

            var allApprenticeships = _fixture.CreateMany<Models.IncentiveApplicationApprenticeship>(10).ToArray();
            allApprenticeships[1].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[2].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[3].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[4].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[5].IncentiveApplicationId = incentives[0].Id;

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.ClawedBack, false)
                .CreateMany(2).ToList();
            pendingPayments[0].DueDate = DateTime.Parse("04-01-2020", new CultureInfo("en-GB"));
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].DueDate = DateTime.Parse("01-12-2020", new CultureInfo("en-GB"));
            pendingPayments[1].EarningType = EarningType.SecondPayment;

            incentives[0].PendingPayments = pendingPayments;

            var learners = _fixture.CreateMany<ApprenticeshipIncentives.Models.Learner>(10).ToList();
            learners[0].ULN = incentives[0].ULN;
            learners[0].ApprenticeshipIncentiveId = incentives[0].Id;
            learners[0].InLearning = false;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);
            _context.ApplicationApprenticeships.AddRange(allApprenticeships);
            _context.Learners.AddRange(learners);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            var application = result.FirstOrDefault(x => x.ULN == incentives[0].ULN);
            application.SecondPaymentStatus.PaymentSentIsEstimated.Should().BeTrue();
        }

        [Test()]
        public async Task Then_second_payment_is_estimated_when_the_payment_has_not_been_made()
        {
            for (int i = 1; i < 31; i++)
            {
                await Then_second_payment_is_as_expected_for_the_payment_and_the_calculated_date_is_set(i, false, true);
            }
            Assert.Pass();
        }

        [Test()]
        public async Task Then_second_payment_is_not_estimated_when_the_payment_has_been_made_and_the_current_date_is_day_27_or_greater()
        {
            for (int i = 27; i < 31; i++)
            {
                await Then_second_payment_is_as_expected_for_the_payment_and_the_calculated_date_is_set(i, true, false);
            }
            Assert.Pass();
        }

        [Test()]
        public async Task Then_second_payment_is_estimated_when_the_payment_has_been_made_and_the_current_date_is_less_than_day_27()
        {
            for (int i = 1; i < 27; i++)
            {
                await Then_second_payment_is_as_expected_for_the_payment_and_the_calculated_date_is_set(i, true, true);
            }
            Assert.Pass();
        }
        private async Task Then_second_payment_is_as_expected_for_the_payment_and_the_calculated_date_is_set(
            int day,
            bool paymentMade,
            bool expected)
        {
            // Arrange            
            _context = new EmployerIncentivesDbContext(_options);
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>(5).ToArray();
            incentives[0].AccountId = accountId;
            incentives[0].AccountLegalEntityId = accountLegalEntityId;

            var allApprenticeships = _fixture.CreateMany<Models.IncentiveApplicationApprenticeship>(10).ToArray();
            allApprenticeships[1].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[2].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[3].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[4].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[5].IncentiveApplicationId = incentives[0].Id;

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.ClawedBack, false)
                .CreateMany(2).ToList();
            pendingPayments[0].DueDate = DateTime.Parse("04-01-2020", new CultureInfo("en-GB"));
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].DueDate = DateTime.Parse("01-12-2020", new CultureInfo("en-GB"));
            pendingPayments[1].EarningType = EarningType.SecondPayment;

            var payments = _fixture
                .Build<Payment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .CreateMany(2).ToList();

            payments[0].PaidDate = new DateTime(pendingPayments[0].DueDate.Year, pendingPayments[0].DueDate.Month, payments[0].PaidDate.Value.Day);

            _mockDateTimeService.Setup(m => m.Now()).Returns(new DateTime(pendingPayments[1].DueDate.Year, 12, day));
            if (!paymentMade)
            {
                payments[1].PaidDate = null;
            }
            else
            {
                payments[1].PaidDate = new DateTime(pendingPayments[1].DueDate.Year, 12, payments[1].PaidDate.Value.Day);
            }
            payments[1].PendingPaymentId = pendingPayments[1].Id;

            incentives[0].PendingPayments = pendingPayments;
            incentives[0].Payments = payments;

            var learners = _fixture.CreateMany<ApprenticeshipIncentives.Models.Learner>(10).ToList();
            learners[0].ULN = incentives[0].ULN;
            learners[0].ApprenticeshipIncentiveId = incentives[0].Id;
            learners[0].InLearning = false;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);
            _context.ApplicationApprenticeships.AddRange(allApprenticeships);
            _context.Learners.AddRange(learners);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            var application = result.FirstOrDefault(x => x.ULN == incentives[0].ULN);
            application.SecondPaymentStatus.PaymentSentIsEstimated.Should().Be(expected);
        }

        [Test]
        public async Task Then_second_payment_is_not_sent_if_the_payment_record_has_not_been_created()
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>(5).ToArray();
            incentives[0].AccountId = accountId;
            incentives[0].AccountLegalEntityId = accountLegalEntityId;

            var allApprenticeships = _fixture.CreateMany<Models.IncentiveApplicationApprenticeship>(10).ToArray();
            allApprenticeships[1].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[2].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[3].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[4].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[5].IncentiveApplicationId = incentives[0].Id;

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.ClawedBack, false)
                .CreateMany(2).ToList();
            pendingPayments[0].DueDate = DateTime.Parse("04-01-2020", new CultureInfo("en-GB"));
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].DueDate = DateTime.Parse("01-12-2020", new CultureInfo("en-GB"));
            pendingPayments[1].EarningType = EarningType.SecondPayment;

            incentives[0].PendingPayments = pendingPayments;

            var learners = _fixture.CreateMany<ApprenticeshipIncentives.Models.Learner>(10).ToList();
            learners[0].ULN = incentives[0].ULN;
            learners[0].ApprenticeshipIncentiveId = incentives[0].Id;
            learners[0].InLearning = false;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);
            _context.ApplicationApprenticeships.AddRange(allApprenticeships);
            _context.Learners.AddRange(learners);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            var application = result.FirstOrDefault(x => x.ULN == incentives[0].ULN);
            application.SecondPaymentStatus.PaymentSent.Should().BeFalse();
        }

        [TestCase(true, true, true, false, true)]
        [TestCase(true, false, true, true, false)]
        [TestCase(false, true, false, false, false)]
        [TestCase(false, false, false, true, false)]
        public async Task Then_the_employment_check_status_reflects_whether_the_payment_validation_results_indicate_a_pass(
            bool firstEmploymentCheckStatus,
            bool secondEmploymentCheckStatus,
            bool firstEmploymentCheckValue,
            bool secondEmploymentCheckValue,
            bool overallEmploymentCheckStatus)
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>(5).ToArray();
            incentives[0].AccountId = accountId;
            incentives[0].AccountLegalEntityId = accountLegalEntityId;

            var allApprenticeships = _fixture.CreateMany<Models.IncentiveApplicationApprenticeship>(10).ToArray();
            allApprenticeships[1].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[2].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[3].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[4].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[5].IncentiveApplicationId = incentives[0].Id;

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.ClawedBack, false)
                .With(p => p.ValidationResults, new List<PendingPaymentValidationResult>())
                .CreateMany(2).ToList();
            pendingPayments[0].DueDate = DateTime.Parse("04-01-2020", new CultureInfo("en-GB"));
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].DueDate = DateTime.Parse("01-12-2020", new CultureInfo("en-GB"));
            pendingPayments[1].EarningType = EarningType.SecondPayment;

            incentives[0].PendingPayments = pendingPayments;

            var learners = _fixture.CreateMany<ApprenticeshipIncentives.Models.Learner>(10).ToList();
            learners[0].ULN = incentives[0].ULN;
            learners[0].ApprenticeshipIncentiveId = incentives[0].Id;
            learners[0].InLearning = false;

            var firstPaymentValidationResult = _fixture.Build<PendingPaymentValidationResult>()
                .With(x => x.Result, firstEmploymentCheckStatus)
                .With(x => x.PendingPaymentId, pendingPayments[0].Id)
                .With(x => x.Step, ValidationStep.EmployedAtStartOfApprenticeship)
                .Create();

            var secondPaymentValidationResult = _fixture.Build<PendingPaymentValidationResult>()
                .With(x => x.Result, secondEmploymentCheckStatus)
                .With(x => x.PendingPaymentId, pendingPayments[0].Id)
                .With(x => x.Step, ValidationStep.EmployedBeforeSchemeStarted)
                .Create();

            incentives[0].PendingPayments.ToList()[0].ValidationResults.Add(firstPaymentValidationResult);
            incentives[0].PendingPayments.ToList()[0].ValidationResults.Add(secondPaymentValidationResult);
            incentives[0].PendingPayments.ToList()[1].ValidationResults.Add(firstPaymentValidationResult);
            incentives[0].PendingPayments.ToList()[1].ValidationResults.Add(secondPaymentValidationResult);

            var employmentCheck1 = _fixture.Build<ApprenticeshipIncentives.Models.EmploymentCheck>()
                .With(x => x.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(x => x.CheckType, EmploymentCheckType.EmployedAtStartOfApprenticeship)
                .With(x => x.Result, firstEmploymentCheckValue)
                .Create();

            var employmentCheck2 = _fixture.Build<ApprenticeshipIncentives.Models.EmploymentCheck>()
                .With(x => x.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(x => x.CheckType, EmploymentCheckType.EmployedBeforeSchemeStarted)
                .With(x => x.Result, secondEmploymentCheckValue)
                .Create();

            incentives[0].EmploymentChecks.Clear();
            incentives[0].EmploymentChecks.Add(employmentCheck1);
            incentives[0].EmploymentChecks.Add(employmentCheck2);

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);
            _context.ApplicationApprenticeships.AddRange(allApprenticeships);
            _context.Learners.AddRange(learners);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            result.Count(x => x.ULN == incentives[0].ULN).Should().Be(1);
            var application = result.FirstOrDefault(x => x.ULN == incentives[0].ULN);
            application.FirstPaymentStatus.EmploymentCheckPassed.Should().NotBeNull();
            application.FirstPaymentStatus.EmploymentCheckPassed.Value.Should().Be(overallEmploymentCheckStatus);
            application.SecondPaymentStatus.EmploymentCheckPassed.Should().NotBeNull();
            application.SecondPaymentStatus.EmploymentCheckPassed.Value.Should().Be(overallEmploymentCheckStatus);
        }

        [Test]
        public async Task Then_the_most_recent_employment_check_payment_validation_is_used_when_more_than_one_validation_has_been_performed()
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>(5).ToArray();
            incentives[0].AccountId = accountId;
            incentives[0].AccountLegalEntityId = accountLegalEntityId;

            var allApprenticeships = _fixture.CreateMany<Models.IncentiveApplicationApprenticeship>(10).ToArray();
            allApprenticeships[1].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[2].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[3].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[4].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[5].IncentiveApplicationId = incentives[0].Id;

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.ClawedBack, false)
                .With(p => p.ValidationResults, new List<PendingPaymentValidationResult>())
                .CreateMany(2).ToList();
            pendingPayments[0].DueDate = DateTime.Parse("04-01-2020", new CultureInfo("en-GB"));
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].DueDate = DateTime.Parse("01-12-2020", new CultureInfo("en-GB"));
            pendingPayments[1].EarningType = EarningType.SecondPayment;

            incentives[0].PendingPayments = pendingPayments;

            var learners = _fixture.CreateMany<ApprenticeshipIncentives.Models.Learner>(10).ToList();
            learners[0].ULN = incentives[0].ULN;
            learners[0].ApprenticeshipIncentiveId = incentives[0].Id;
            learners[0].InLearning = false;

            var firstPaymentValidationResult1 = _fixture.Build<PendingPaymentValidationResult>()
                .With(x => x.Result, false)
                .With(x => x.PendingPaymentId, pendingPayments[0].Id)
                .With(x => x.Step, ValidationStep.EmployedAtStartOfApprenticeship)
                .With(x => x.CreatedDateUtc, new DateTime(2021, 12, 01))
                .Create();

            var secondPaymentValidationResult1 = _fixture.Build<PendingPaymentValidationResult>()
                .With(x => x.Result, false)
                .With(x => x.PendingPaymentId, pendingPayments[0].Id)
                .With(x => x.Step, ValidationStep.EmployedBeforeSchemeStarted)
                .With(x => x.CreatedDateUtc, new DateTime(2021, 12, 01))
                .Create();

            var firstPaymentValidationResult2 = _fixture.Build<PendingPaymentValidationResult>()
                .With(x => x.Result, true)
                .With(x => x.PendingPaymentId, pendingPayments[0].Id)
                .With(x => x.Step, ValidationStep.EmployedAtStartOfApprenticeship)
                .With(x => x.CreatedDateUtc, new DateTime(2021, 12, 02))
                .Create();

            var secondPaymentValidationResult2 = _fixture.Build<PendingPaymentValidationResult>()
                .With(x => x.Result, true)
                .With(x => x.PendingPaymentId, pendingPayments[0].Id)
                .With(x => x.Step, ValidationStep.EmployedBeforeSchemeStarted)
                .With(x => x.CreatedDateUtc, new DateTime(2021, 12, 02))
                .Create();

            incentives[0].PendingPayments.ToList()[0].ValidationResults.Add(firstPaymentValidationResult1);
            incentives[0].PendingPayments.ToList()[0].ValidationResults.Add(secondPaymentValidationResult1);
            incentives[0].PendingPayments.ToList()[1].ValidationResults.Add(firstPaymentValidationResult1);
            incentives[0].PendingPayments.ToList()[1].ValidationResults.Add(secondPaymentValidationResult1);
            incentives[0].PendingPayments.ToList()[0].ValidationResults.Add(firstPaymentValidationResult2);
            incentives[0].PendingPayments.ToList()[0].ValidationResults.Add(secondPaymentValidationResult2);
            incentives[0].PendingPayments.ToList()[1].ValidationResults.Add(firstPaymentValidationResult2);
            incentives[0].PendingPayments.ToList()[1].ValidationResults.Add(secondPaymentValidationResult2);

            var employmentCheck1 = _fixture.Build<ApprenticeshipIncentives.Models.EmploymentCheck>()
                .With(x => x.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(x => x.CheckType, EmploymentCheckType.EmployedAtStartOfApprenticeship)
                .With(x => x.Result, true)
                .Create();

            var employmentCheck2 = _fixture.Build<ApprenticeshipIncentives.Models.EmploymentCheck>()
                .With(x => x.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(x => x.CheckType, EmploymentCheckType.EmployedBeforeSchemeStarted)
                .With(x => x.Result, false)
                .Create();

            incentives[0].EmploymentChecks.Clear();
            incentives[0].EmploymentChecks.Add(employmentCheck1);
            incentives[0].EmploymentChecks.Add(employmentCheck2);

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);
            _context.ApplicationApprenticeships.AddRange(allApprenticeships);
            _context.Learners.AddRange(learners);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            result.Count(x => x.ULN == incentives[0].ULN).Should().Be(1);
            var application = result.FirstOrDefault(x => x.ULN == incentives[0].ULN);
            application.FirstPaymentStatus.EmploymentCheckPassed.Should().BeTrue();
            application.SecondPaymentStatus.EmploymentCheckPassed.Should().BeTrue();
        }

        [Test]
        public async Task Then_the_employment_check_is_not_set_if_there_are_no_payment_validation_results()
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>(5).ToArray();
            incentives[0].AccountId = accountId;
            incentives[0].AccountLegalEntityId = accountLegalEntityId;

            var allApprenticeships = _fixture.CreateMany<Models.IncentiveApplicationApprenticeship>(10).ToArray();
            allApprenticeships[1].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[2].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[3].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[4].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[5].IncentiveApplicationId = incentives[0].Id;

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.ClawedBack, false)
                .With(p => p.ValidationResults, new List<PendingPaymentValidationResult>())
                .CreateMany(2).ToList();
            pendingPayments[0].DueDate = DateTime.Parse("04-01-2020", new CultureInfo("en-GB"));
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].DueDate = DateTime.Parse("01-12-2020", new CultureInfo("en-GB"));
            pendingPayments[1].EarningType = EarningType.SecondPayment;

            incentives[0].PendingPayments = pendingPayments;

            var learners = _fixture.CreateMany<ApprenticeshipIncentives.Models.Learner>(10).ToList();
            learners[0].ULN = incentives[0].ULN;
            learners[0].ApprenticeshipIncentiveId = incentives[0].Id;
            learners[0].InLearning = false;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);
            _context.ApplicationApprenticeships.AddRange(allApprenticeships);
            _context.Learners.AddRange(learners);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            result.Count(x => x.ULN == incentives[0].ULN).Should().Be(1);
            var application = result.FirstOrDefault(x => x.ULN == incentives[0].ULN);
            application.FirstPaymentStatus.EmploymentCheckPassed.Should().BeNull();
            application.SecondPaymentStatus.EmploymentCheckPassed.Should().BeNull();
        }

        [Test]
        public async Task Then_the_employment_check_is_not_set_if_the_payment_validation_results_do_not_include_the_employment_checks()
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>(5).ToArray();
            incentives[0].AccountId = accountId;
            incentives[0].AccountLegalEntityId = accountLegalEntityId;

            var allApprenticeships = _fixture.CreateMany<Models.IncentiveApplicationApprenticeship>(10).ToArray();
            allApprenticeships[1].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[2].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[3].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[4].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[5].IncentiveApplicationId = incentives[0].Id;

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.ClawedBack, false)
                .With(p => p.ValidationResults, new List<PendingPaymentValidationResult>())
                .CreateMany(2).ToList();
            pendingPayments[0].DueDate = DateTime.Parse("04-01-2020", new CultureInfo("en-GB"));
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].DueDate = DateTime.Parse("01-12-2020", new CultureInfo("en-GB"));
            pendingPayments[1].EarningType = EarningType.SecondPayment;

            incentives[0].PendingPayments = pendingPayments;

            var learners = _fixture.CreateMany<ApprenticeshipIncentives.Models.Learner>(10).ToList();
            learners[0].ULN = incentives[0].ULN;
            learners[0].ApprenticeshipIncentiveId = incentives[0].Id;
            learners[0].InLearning = false;

            var firstPaymentValidationResult = _fixture.Build<PendingPaymentValidationResult>()
                .With(x => x.Result, false)
                .With(x => x.PendingPaymentId, pendingPayments[0].Id)
                .With(x => x.Step, ValidationStep.HasBankDetails)
                .With(x => x.CreatedDateUtc, new DateTime(2021, 12, 01))
                .Create();

            var secondPaymentValidationResult = _fixture.Build<PendingPaymentValidationResult>()
                .With(x => x.Result, false)
                .With(x => x.PendingPaymentId, pendingPayments[0].Id)
                .With(x => x.Step, ValidationStep.HasLearningRecord)
                .With(x => x.CreatedDateUtc, new DateTime(2021, 12, 01))
                .Create();

            incentives[0].PendingPayments.ToList()[0].ValidationResults.Add(firstPaymentValidationResult);
            incentives[0].PendingPayments.ToList()[0].ValidationResults.Add(secondPaymentValidationResult);
            incentives[0].PendingPayments.ToList()[1].ValidationResults.Add(firstPaymentValidationResult);
            incentives[0].PendingPayments.ToList()[1].ValidationResults.Add(secondPaymentValidationResult);

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);
            _context.ApplicationApprenticeships.AddRange(allApprenticeships);
            _context.Learners.AddRange(learners);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            result.Count(x => x.ULN == incentives[0].ULN).Should().Be(1);
            var application = result.FirstOrDefault(x => x.ULN == incentives[0].ULN);
            application.FirstPaymentStatus.EmploymentCheckPassed.Should().BeNull();
            application.SecondPaymentStatus.EmploymentCheckPassed.Should().BeNull();
        }

        [Test]
        public async Task Then_the_employment_check_is_not_set_if_the_validation_result_is_false_and_no_employment_check_record_has_been_created()
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>(5).ToArray();
            incentives[0].AccountId = accountId;
            incentives[0].AccountLegalEntityId = accountLegalEntityId;

            var allApprenticeships = _fixture.CreateMany<Models.IncentiveApplicationApprenticeship>(10).ToArray();
            allApprenticeships[1].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[2].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[3].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[4].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[5].IncentiveApplicationId = incentives[0].Id;

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.ClawedBack, false)
                .With(p => p.ValidationResults, new List<PendingPaymentValidationResult>())
                .CreateMany(2).ToList();
            pendingPayments[0].DueDate = DateTime.Parse("04-01-2020", new CultureInfo("en-GB"));
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].DueDate = DateTime.Parse("01-12-2020", new CultureInfo("en-GB"));
            pendingPayments[1].EarningType = EarningType.SecondPayment;

            incentives[0].PendingPayments = pendingPayments;

            var learners = _fixture.CreateMany<ApprenticeshipIncentives.Models.Learner>(10).ToList();
            learners[0].ULN = incentives[0].ULN;
            learners[0].ApprenticeshipIncentiveId = incentives[0].Id;
            learners[0].InLearning = false;

            var firstPaymentValidationResult = _fixture.Build<PendingPaymentValidationResult>()
                .With(x => x.Result, false)
                .With(x => x.PendingPaymentId, pendingPayments[0].Id)
                .With(x => x.Step, ValidationStep.EmployedAtStartOfApprenticeship)
                .Create();

            var secondPaymentValidationResult = _fixture.Build<PendingPaymentValidationResult>()
                .With(x => x.Result, false)
                .With(x => x.PendingPaymentId, pendingPayments[0].Id)
                .With(x => x.Step, ValidationStep.EmployedBeforeSchemeStarted)
                .Create();

            incentives[0].PendingPayments.ToList()[0].ValidationResults.Add(firstPaymentValidationResult);
            incentives[0].PendingPayments.ToList()[0].ValidationResults.Add(secondPaymentValidationResult);
            incentives[0].PendingPayments.ToList()[1].ValidationResults.Add(firstPaymentValidationResult);
            incentives[0].PendingPayments.ToList()[1].ValidationResults.Add(secondPaymentValidationResult);

            incentives[0].EmploymentChecks.Clear();

            var additionalValidationResult1 = _fixture.Build<PendingPaymentValidationResult>()
                .With(x => x.PendingPaymentId, pendingPayments[0].Id)
                .Create();
            var additionalValidationResult2 = _fixture.Build<PendingPaymentValidationResult>()
                .With(x => x.PendingPaymentId, pendingPayments[1].Id)
                .Create();

            incentives[0].PendingPayments.ToList()[0].ValidationResults.Add(additionalValidationResult1);
            incentives[0].PendingPayments.ToList()[1].ValidationResults.Add(additionalValidationResult2);

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);
            _context.ApplicationApprenticeships.AddRange(allApprenticeships);
            _context.Learners.AddRange(learners);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            result.Count(x => x.ULN == incentives[0].ULN).Should().Be(1);
            var application = result.FirstOrDefault(x => x.ULN == incentives[0].ULN);
            application.FirstPaymentStatus.EmploymentCheckPassed.Should().BeNull();
            application.SecondPaymentStatus.EmploymentCheckPassed.Should().BeNull();
        }

        [Test]
        public async Task Then_the_employment_check_is_not_set_if_the_validation_result_is_false_and_the_employment_check_record_result_is_null()
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>(5).ToArray();
            incentives[0].AccountId = accountId;
            incentives[0].AccountLegalEntityId = accountLegalEntityId;

            var allApprenticeships = _fixture.CreateMany<Models.IncentiveApplicationApprenticeship>(10).ToArray();
            allApprenticeships[1].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[2].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[3].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[4].IncentiveApplicationId = incentives[0].Id;
            allApprenticeships[5].IncentiveApplicationId = incentives[0].Id;

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.ClawedBack, false)
                .With(p => p.ValidationResults, new List<PendingPaymentValidationResult>())
                .CreateMany(2).ToList();
            pendingPayments[0].DueDate = DateTime.Parse("04-01-2020", new CultureInfo("en-GB"));
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].DueDate = DateTime.Parse("01-12-2020", new CultureInfo("en-GB"));
            pendingPayments[1].EarningType = EarningType.SecondPayment;

            incentives[0].PendingPayments = pendingPayments;

            var learners = _fixture.CreateMany<ApprenticeshipIncentives.Models.Learner>(10).ToList();
            learners[0].ULN = incentives[0].ULN;
            learners[0].ApprenticeshipIncentiveId = incentives[0].Id;
            learners[0].InLearning = false;

            var firstPaymentValidationResult = _fixture.Build<PendingPaymentValidationResult>()
                .With(x => x.Result, false)
                .With(x => x.PendingPaymentId, pendingPayments[0].Id)
                .With(x => x.Step, ValidationStep.EmployedAtStartOfApprenticeship)
                .Create();

            var secondPaymentValidationResult = _fixture.Build<PendingPaymentValidationResult>()
                .With(x => x.Result, false)
                .With(x => x.PendingPaymentId, pendingPayments[0].Id)
                .With(x => x.Step, ValidationStep.EmployedBeforeSchemeStarted)
                .Create();

            incentives[0].PendingPayments.ToList()[0].ValidationResults.Add(firstPaymentValidationResult);
            incentives[0].PendingPayments.ToList()[0].ValidationResults.Add(secondPaymentValidationResult);
            incentives[0].PendingPayments.ToList()[1].ValidationResults.Add(firstPaymentValidationResult);
            incentives[0].PendingPayments.ToList()[1].ValidationResults.Add(secondPaymentValidationResult);

            var additionalValidationResult1 = _fixture.Build<PendingPaymentValidationResult>()
                .With(x => x.PendingPaymentId, pendingPayments[0].Id)
                .Create();
            var additionalValidationResult2 = _fixture.Build<PendingPaymentValidationResult>()
                .With(x => x.PendingPaymentId, pendingPayments[1].Id)
                .Create();

            incentives[0].PendingPayments.ToList()[0].ValidationResults.Add(additionalValidationResult1);
            incentives[0].PendingPayments.ToList()[1].ValidationResults.Add(additionalValidationResult2);

            var employmentCheck1 = _fixture.Build<ApprenticeshipIncentives.Models.EmploymentCheck>()
                .With(x => x.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(x => x.CheckType, EmploymentCheckType.EmployedAtStartOfApprenticeship)
                .Without(x => x.Result)
                .Create();

            var employmentCheck2 = _fixture.Build<ApprenticeshipIncentives.Models.EmploymentCheck>()
                .With(x => x.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(x => x.CheckType, EmploymentCheckType.EmployedBeforeSchemeStarted)
                .Without(x => x.Result)
                .Create();
            incentives[0].EmploymentChecks.Clear();
            incentives[0].EmploymentChecks.Add(employmentCheck1);
            incentives[0].EmploymentChecks.Add(employmentCheck2);

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);
            _context.ApplicationApprenticeships.AddRange(allApprenticeships);
            _context.Learners.AddRange(learners);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            result.Count(x => x.ULN == incentives[0].ULN).Should().Be(1);
            var application = result.FirstOrDefault(x => x.ULN == incentives[0].ULN);
            application.FirstPaymentStatus.EmploymentCheckPassed.Should().BeNull();
            application.SecondPaymentStatus.EmploymentCheckPassed.Should().BeNull();
        }
        [TestCase(false, false, true)]
        [TestCase(true, false, true)]
        public async Task When_IsInLearning_called_should_return_true_if_IsInLearning_is_false_and_there_is_an_override(bool inLearning, bool hasExpired, bool expectation)
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>(5).ToArray();
            incentives[0].AccountId = accountId;
            incentives[0].AccountLegalEntityId = accountLegalEntityId;
            incentives[0].PausePayments = false;
            incentives[0].MinimumAgreementVersion = allAccounts[0].SignedAgreementVersion;

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.ClawedBack, false)
                .With(p => p.ValidationResults, new List<PendingPaymentValidationResult>())
                .CreateMany(2).ToList();
            pendingPayments[0].DueDate = DateTime.Parse("04-01-2020", new CultureInfo("en-GB"));
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].DueDate = DateTime.Parse("01-12-2020", new CultureInfo("en-GB"));
            pendingPayments[1].EarningType = EarningType.SecondPayment;

            var learners = _fixture.CreateMany<ApprenticeshipIncentives.Models.Learner>(10).ToList();
            learners[0].ULN = incentives[0].ULN;
            learners[0].InLearning = inLearning;

            var existingOverride = _fixture
                .Build<ValidationOverride>()
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.Step, ValidationStep.IsInLearning)
                .With(p => p.ExpiryDate == DateTime.UtcNow.AddDays(1))
                .Create();

            var validationOverrides = _fixture
                .Build<ValidationOverride>()
                .CreateMany(5).ToList();

            validationOverrides.Add(existingOverride);

            incentives[0].PendingPayments = pendingPayments;
            incentives[0].ValidationOverrides = validationOverrides;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);
            _context.ValidationOverrides.AddRange(validationOverrides);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            result.Count(x => x.ULN == incentives[0].ULN).Should().Be(1);
            var application = result.FirstOrDefault(x => x.ULN == incentives[0].ULN);
            application.FirstPaymentStatus.InLearning.Should().Be(expectation);
            application.SecondPaymentStatus.InLearning.Should().Be(expectation);
        }
        [Test]
        public async Task When_IsInLearning_called_return_false_if_there_is_no_override_if_inLearning_returns_false()
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>(5).ToArray();
            incentives[0].AccountId = accountId;
            incentives[0].AccountLegalEntityId = accountLegalEntityId;
            incentives[0].PausePayments = false;
            incentives[0].MinimumAgreementVersion = allAccounts[0].SignedAgreementVersion;

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.ClawedBack, false)
                .With(p => p.ValidationResults, new List<PendingPaymentValidationResult>())
                .CreateMany(2).ToList();
            pendingPayments[0].DueDate = DateTime.Parse("04-01-2020", new CultureInfo("en-GB"));
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].DueDate = DateTime.Parse("01-12-2020", new CultureInfo("en-GB"));
            pendingPayments[1].EarningType = EarningType.SecondPayment;

            var validationOverrides = _fixture
                .Build<ValidationOverride>()
                .CreateMany(5).ToList();

            incentives[0].PendingPayments = pendingPayments;
            incentives[0].ValidationOverrides = validationOverrides;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);
            _context.ValidationOverrides.AddRange(validationOverrides);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            result.Count(x => x.ULN == incentives[0].ULN).Should().Be(1);
            var application = result.FirstOrDefault(x => x.ULN == incentives[0].ULN);
            application.FirstPaymentStatus.InLearning.Should().BeFalse();
            application.SecondPaymentStatus.InLearning.Should().BeFalse();
        }
            [Test]
        public async Task When_HasDataLock_is_called_return_false_if_there_is_override()
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>(5).ToArray();
            incentives[0].AccountId = accountId;
            incentives[0].AccountLegalEntityId = accountLegalEntityId;
            incentives[0].PausePayments = false;
            incentives[0].MinimumAgreementVersion = allAccounts[0].SignedAgreementVersion;

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.ClawedBack, false)
                .With(p => p.ValidationResults, new List<PendingPaymentValidationResult>())
                .CreateMany(2).ToList();
            pendingPayments[0].DueDate = DateTime.Parse("04-01-2020", new CultureInfo("en-GB"));
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].DueDate = DateTime.Parse("01-12-2020", new CultureInfo("en-GB"));
            pendingPayments[1].EarningType = EarningType.SecondPayment;

            var existingOverride = _fixture
                .Build<ValidationOverride>()
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .With(p => p.Step, ValidationStep.HasNoDataLocks)
                .Without(p => p.ExpiryDate)
                .Create();

            var validationOverrides = _fixture
                .Build<ValidationOverride>()
                .CreateMany(5).ToList();

            validationOverrides.Add(existingOverride);

            incentives[0].PendingPayments = pendingPayments;
            incentives[0].ValidationOverrides = validationOverrides;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);
            _context.ValidationOverrides.AddRange(validationOverrides);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            result.Count(x => x.ULN == incentives[0].ULN).Should().Be(1);
            var application = result.FirstOrDefault(x => x.ULN == incentives[0].ULN);
            application.FirstPaymentStatus.InLearning.Should().BeFalse();
            application.SecondPaymentStatus.InLearning.Should().BeFalse();
        }
        [Test]
        public async Task When_HasDataLock_called_return_true_if_there_is_no_override()
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(1).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var incentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>(1).ToArray();
            incentives[0].AccountId = accountId;
            incentives[0].AccountLegalEntityId = accountLegalEntityId;

            var existingOverride = _fixture
                .Build<ValidationOverride>()
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .Without(p => p.ExpiryDate)
                .Create();

            var validationOverrides = _fixture
                .Build<ValidationOverride>()
                .CreateMany(5).ToList();

            validationOverrides.Add(existingOverride);

            incentives[0].ValidationOverrides = validationOverrides;


            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);
            _context.ValidationOverrides.AddRange(validationOverrides);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            result.Count(x => x.ULN == incentives[0].ULN).Should().Be(1);
            var application = result.FirstOrDefault(x => x.ULN == incentives[0].ULN);
            application.FirstPaymentStatus.HasDataLock.Should().BeFalse();
            application.SecondPaymentStatus.HasDataLock.Should().BeFalse();
        }
    }
}
