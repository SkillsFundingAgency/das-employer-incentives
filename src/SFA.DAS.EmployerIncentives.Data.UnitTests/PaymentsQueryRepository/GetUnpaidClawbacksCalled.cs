using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.PaymentsQueryRepository
{
    public class GetUnpaidClawbacksCalled
    {
        private EmployerIncentivesDbContext _context;
        private Fixture _fixture;
        private IPaymentsQueryRepository _sut;
        private ClawbackPayment _clawback1;
        private PendingPayment _pendingPayment1;
        private Models.Account _account1;
        private ApprenticeshipIncentives.Models.ApprenticeshipIncentive _apprenticeshipIncentive1;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _context = new EmployerIncentivesDbContext(options);

            _sut = new ApprenticeshipIncentives.PaymentsQueryRepository(new Lazy<EmployerIncentivesDbContext>(_context));

            SetupAccount();
            SetupApprenticeshipIncentives();
            SetupPendingPayments();
            SetupClawbacks();
        }

        [TearDown]
        public void CleanUp()
        {
            _context.Dispose();
        }

        [TestCase(1, true)]
        [TestCase(2, false)]
        public async Task Then_should_return_only_records_for_account_legal_entity(long accountLegalEntityId, bool expected)
        {
            var clawbacks = await _sut.GetUnpaidClawbacks(accountLegalEntityId);

            (clawbacks.Count > 0).Should().Be(expected);
        }

        [Test]
        public async Task Then_should_return_only_records_for_clawbacks_which_havent_been_sent()
        {
            var clawbacks = await _sut.GetUnpaidClawbacks(1);

            clawbacks.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_should_return_records_with_values_mapped()
        {
            var clawbacks = await _sut.GetUnpaidClawbacks(1);

            clawbacks.Count.Should().Be(1);
            clawbacks[0].PaymentId.Should().Be(_clawback1.Id);
            clawbacks[0].ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive1.Id);
            clawbacks[0].AccountLegalEntityId.Should().Be(_account1.AccountLegalEntityId);
            clawbacks[0].VendorId.Should().Be(_account1.VrfVendorId);
            clawbacks[0].DueDate.Should().Be(_pendingPayment1.DueDate);
            clawbacks[0].EarningType.Should().Be(_pendingPayment1.EarningType);
            clawbacks[0].ULN.Should().Be(_apprenticeshipIncentive1.ULN);
            clawbacks[0].HashedLegalEntityId.Should().Be(_account1.HashedLegalEntityId);
        }

        private void SetupPendingPayments()
        {

            _pendingPayment1 = _fixture.Build<PendingPayment>().Create();


            var pendingPayments = new List<PendingPayment>
            {
                _pendingPayment1
            };

            _context.PendingPayments.AddRange(pendingPayments);
            _context.SaveChanges();
        }

        private void SetupApprenticeshipIncentives()
        {
            _apprenticeshipIncentive1 = _fixture.Build<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>()
                .With(x => x.AccountId, _account1.Id)
                .With(x => x.AccountLegalEntityId, _account1.AccountLegalEntityId)
                .Create();

            var apprenticeshipIncentives = new List<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>()
            {
                _apprenticeshipIncentive1
            };

            _context.ApprenticeshipIncentives.AddRange(apprenticeshipIncentives);
            _context.SaveChanges();
        }

        private void SetupAccount()
        {
            _account1 = _fixture.Build<Models.Account>().With(x=>x.AccountLegalEntityId, 1).Create();

            var accounts = new List<Models.Account>()
            {
                _account1
            };

            _context.Accounts.AddRange(accounts);
            _context.SaveChanges();
        }

        private void SetupClawbacks()
        {
            _clawback1 = _fixture.Build<ClawbackPayment>()
                .With(x => x.ApprenticeshipIncentiveId, _apprenticeshipIncentive1.Id)
                .With(x => x.PendingPaymentId, _pendingPayment1.Id)
                .With(x => x.AccountId, _account1.Id)
                .With(x => x.AccountLegalEntityId, _account1.AccountLegalEntityId)
                .Without(x=>x.DateClawbackSent)
                .Create();

            var sentClawback = _fixture.Build<ClawbackPayment>()
                .With(x => x.ApprenticeshipIncentiveId, _apprenticeshipIncentive1.Id)
                .With(x => x.PendingPaymentId, _pendingPayment1.Id)
                .With(x => x.AccountId, _account1.Id)
                .With(x => x.AccountLegalEntityId, _account1.AccountLegalEntityId)
                .Create();

            var clawbacks = new List<ClawbackPayment>()
            {
                _clawback1,
                sentClawback
            };

            _context.ClawbackPayments.AddRange(clawbacks);
            _context.SaveChanges();
        }
    }
}