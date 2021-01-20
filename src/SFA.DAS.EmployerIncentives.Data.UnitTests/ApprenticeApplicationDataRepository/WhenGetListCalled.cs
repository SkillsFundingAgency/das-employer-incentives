using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
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
        public async Task And_incentive_has_2_pending_payments_Then_data_is_fetched_from_the_database()
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
                LearnerMatchNotFound = false,
                PaymentAmount = pendingPayments[0].Amount,
                PaymentDate = DateTime.Parse("04-02-2020", new CultureInfo("en-GB"))
            };
            result[0].FirstPaymentStatus.Should().BeEquivalentTo(expectedFirstPaymentStatus);

            var expectedSecondPaymentStatus = new PaymentStatusDto
            {
                LearnerMatchNotFound = false,
                PaymentAmount = pendingPayments[1].Amount,
                PaymentDate = DateTime.Parse("01-01-2021", new CultureInfo("en-GB"))
            };
            result[0].SecondPaymentStatus.Should().BeEquivalentTo(expectedSecondPaymentStatus);
        }

        [Test]
        public async Task And_incentive_has_1_pending_payment_Then_data_is_fetched_from_the_database()
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
                .CreateMany(1).ToList();
            pendingPayments[0].DueDate = DateTime.Parse("04-01-2020", new CultureInfo("en-GB"));
            pendingPayments[0].EarningType = EarningType.FirstPayment;

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
                LearnerMatchNotFound = false,
                PaymentAmount = pendingPayments[0].Amount,
                PaymentDate = DateTime.Parse("04-02-2020", new CultureInfo("en-GB"))
            };
            result[0].FirstPaymentStatus.Should().BeEquivalentTo(expectedFirstPaymentStatus);

        }

        [Test]
        public async Task And_incentive_has_no_pending_payments_Then_data_is_fetched_from_the_database()
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


            incentives[0].PendingPayments = new List<PendingPayment>();

            _context.Accounts.AddRange(allAccounts);
            _context.ApprenticeshipIncentives.AddRange(incentives);
            _context.ApplicationApprenticeships.AddRange(allApprenticeships);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            result.Length.Should().Be(1);
            result.All(x => x.AccountId == accountId).Should().BeTrue();
        }
    }
}
