using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;
using System;
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

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _context = new EmployerIncentivesDbContext(options);

            _sut = new Data.ApprenticeApplicationDataRepository(new Lazy<EmployerIncentivesDbContext>(_context));
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
                PaymentAmount = pendingPayments[0].Amount,
                PaymentDate = DateTime.Parse("04-02-2020", new CultureInfo("en-GB"))
            };
            result[0].FirstPaymentStatus.Should().BeEquivalentTo(expectedFirstPaymentStatus);

            var expectedSecondPaymentStatus = new PaymentStatusDto
            {
                LearnerMatchFound = false,
                PaymentAmount = pendingPayments[1].Amount,
                PaymentDate = DateTime.Parse("01-01-2021", new CultureInfo("en-GB"))
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
    }
}
