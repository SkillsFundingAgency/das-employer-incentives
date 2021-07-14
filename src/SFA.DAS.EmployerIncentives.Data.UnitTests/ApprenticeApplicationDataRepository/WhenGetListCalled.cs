using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

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
        private List<Domain.ValueObjects.CollectionPeriod> _collectionPeriods;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _context = new EmployerIncentivesDbContext(_options);

            _mockDateTimeService = new Mock<IDateTimeService>();
            _mockCollectionCalendarService = new Mock<ICollectionCalendarService>();

            
            _collectionPeriods = new List<Domain.ValueObjects.CollectionPeriod>()
            {
                new Domain.ValueObjects.CollectionPeriod(1, (byte)DateTime.Now.Month, (short)DateTime.Now.Year, DateTime.Now.AddDays(-1), DateTime.Now, (short)DateTime.Now.Year, true, false),
                new Domain.ValueObjects.CollectionPeriod(2, (byte)DateTime.Now.AddMonths(1).Month, (short)DateTime.Now.AddMonths(1).Year, DateTime.Now.AddMonths(1).AddDays(-1), DateTime.Now.AddMonths(1), (short)DateTime.Now.AddMonths(1).Year, false, false)
            };

            _mockCollectionCalendarService
                .Setup(m => m.Get())
                .ReturnsAsync(new Domain.ValueObjects.CollectionCalendar(_collectionPeriods));

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
                .CreateMany(2).ToList();
            pendingPayments[0].DueDate = DateTime.Parse("04-01-2020", new CultureInfo("en-GB"));
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].DueDate = DateTime.Parse("01-12-2020", new CultureInfo("en-GB"));
            pendingPayments[1].EarningType = EarningType.SecondPayment;

            incentives[0].PendingPayments = pendingPayments;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);
            _context.ApplicationApprenticeships.AddRange(allApprenticeships);

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
                PaymentDate = DateTime.Parse("01-01-2021", new CultureInfo("en-GB")),
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
                .CreateMany(2).ToList();
            pendingPayments[0].DueDate = DateTime.Parse("04-01-2020", new CultureInfo("en-GB"));
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].DueDate = DateTime.Parse("01-12-2020", new CultureInfo("en-GB"));
            pendingPayments[1].EarningType = EarningType.SecondPayment;

            incentives[0].PendingPayments = pendingPayments;            

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);
            _context.ApplicationApprenticeships.AddRange(allApprenticeships);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            foreach(var application in result)
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
            _context.ApplicationApprenticeships.AddRange(allApprenticeships);
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
            _context.ApplicationApprenticeships.AddRange(allApprenticeships);
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
            _context.ApplicationApprenticeships.AddRange(allApprenticeships);
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
            _context.ApplicationApprenticeships.AddRange(allApprenticeships);
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
            _context.ApplicationApprenticeships.AddRange(allApprenticeships);
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
            _context.ApplicationApprenticeships.AddRange(allApprenticeships);
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
                .CreateMany(2).ToList();

            pendingPayments[0].DueDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month,4);
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
                .CreateMany(2).ToList();

            pendingPayments[0].DueDate = _collectionPeriods.Single(p => p.PeriodNumber == 1).OpenDate.AddDays(-1);
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
            application.FirstPaymentStatus.PaymentDate.Value.Year.Should().Be(_collectionPeriods.Single(p => p.PeriodNumber == 2).AcademicYear);
            application.FirstPaymentStatus.PaymentDate.Value.Month.Should().Be(_collectionPeriods.Single(p => p.PeriodNumber == 2).CalendarMonth);
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
            _context.ApplicationApprenticeships.AddRange(allApprenticeships);
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
            for(int i = 1; i < 27; i++ )
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
            _context.ApplicationApprenticeships.AddRange(allApprenticeships);
            _context.Learners.AddRange(learners);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            var application = result.FirstOrDefault(x => x.ULN == incentives[0].ULN);
            application.Should().NotBeNull();
            application.FirstPaymentStatus.PaymentSentIsEstimated.Should().Be(expected);
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

            var allApprenticeships = _fixture.CreateMany<Models.IncentiveApplicationApprenticeship>(1).ToArray();
            allApprenticeships[0].IncentiveApplicationId = incentives[0].Id;

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentives[0].Id)
                .CreateMany(2).ToList();
            pendingPayments[0].EarningType = EarningType.FirstPayment;
            pendingPayments[1].EarningType = EarningType.SecondPayment;

            incentives[0].PendingPayments = pendingPayments;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);
            _context.ApplicationApprenticeships.AddRange(allApprenticeships);

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
                .With(p => p.Status, apprenticehipStopped? IncentiveStatus.Stopped: IncentiveStatus.Active)
                .Create();

            var apprenticeship = _fixture.Build<Models.IncentiveApplicationApprenticeship>()
                .With(p => p.IncentiveApplicationId, incentive.Id)
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
                .Create());
            }
           
            incentive.PendingPayments = pendingPayments;

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentive);
            _context.ApplicationApprenticeships.AddRange(apprenticeship);

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
                    .Create(),
                _fixture
                    .Build<PendingPayment>()
                    .With(p => p.AccountId, accountId)
                    .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                    .With(p => p.ApprenticeshipIncentiveId, incentive.Id)
                    .With(p => p.EarningType, EarningType.SecondPayment)
                    .Create()
            };

            var payments = _fixture
                .Build<Payment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentive.Id)
                .With(p => p.PendingPaymentId, pendingPayments[0].Id)
                .CreateMany(2).ToList();
            payments[0].PaidDate = new DateTime(pendingPayments[0].DueDate.Year, 1, payments[0].PaidDate.Value.Day);

            var clawback = _fixture.Build<ClawbackPayment>()
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentive.Id)
                .With(p => p.AccountId, accountId)
                .With(p => p.PaymentId, payments[0].Id)
                .With(p => p.PendingPaymentId, pendingPayments[0].Id)
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
            result[0].FirstClawbackStatus.Should().NotBeNull();
            result[0].FirstClawbackStatus.ClawbackAmount.Should().Be(clawback.Amount);
            result[0].FirstClawbackStatus.ClawbackDate.Should().Be(clawback.DateClawbackSent.Value);
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
                    .Create(),
                _fixture
                    .Build<PendingPayment>()
                    .With(p => p.AccountId, accountId)
                    .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                    .With(p => p.ApprenticeshipIncentiveId, incentive.Id)
                    .With(p => p.EarningType, EarningType.SecondPayment)
                    .Create()
            };

            var payments = _fixture
                .Build<Payment>()
                .With(p => p.AccountId, accountId)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, incentive.Id)
                .With(p => p.PendingPaymentId, pendingPayments[1].Id)
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
            result[0].SecondClawbackStatus.Should().NotBeNull();
            result[0].SecondClawbackStatus.ClawbackAmount.Should().Be(clawback.Amount);
            result[0].SecondClawbackStatus.ClawbackDate.Should().Be(clawback.DateClawbackSent.Value);
        }
    }
}
