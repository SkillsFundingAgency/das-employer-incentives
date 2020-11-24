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

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.PayableLegalEntityQueryRepository
{
    public class WhenGettingPaymentsToSendForAccountLegalEntity
    {
        private EmployerIncentivesDbContext _context;
        private Fixture _fixture;
        private IPayableLegalEntityQueryRepository _sut;
        short _collectionPeriodYear = 2020;
        byte _collectionPeriodMonth = 5;
        private Payment _payment1;
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

            _sut = new ApprenticeshipIncentives.PayableLegalEntityQueryRepository(new Lazy<EmployerIncentivesDbContext>(_context));

            SetupAccount();
            SetupApprenticeshipIncentives();
            SetupPendingPayments();
            SetupPayments();
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
            var payments = await _sut.GetPaymentsToSendForAccountLegalEntity(accountLegalEntityId);

            (payments.Count > 0).Should().Be(expected);
        }

        [Test]
        public async Task Then_should_return_only_records_for_payments_which_havent_been_sent()
        {
            var payments = await _sut.GetPaymentsToSendForAccountLegalEntity(1);

            payments.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_should_return_records_with_values_mapped()
        {
            var payments = await _sut.GetPaymentsToSendForAccountLegalEntity(1);

            payments.Count.Should().Be(1);
            payments[0].PaymentId.Should().Be(_payment1.Id);
            payments[0].ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive1.Id);
            payments[0].AccountLegalEntityId.Should().Be(_account1.AccountLegalEntityId);
            payments[0].VendorId.Should().Be(_account1.VrfVendorId);
            payments[0].DueDate.Should().Be(_pendingPayment1.DueDate);
            payments[0].PaymentSequence.Should().Be("First");
            payments[0].ULN.Should().Be(_apprenticeshipIncentive1.ULN);
            payments[0].HashedLegalEntityId.Should().Be(_account1.HashedLegalEntityId);
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

        private void SetupPayments()
        {
            _payment1 = _fixture.Build<Payment>()
                .With(x => x.ApprenticeshipIncentiveId, _apprenticeshipIncentive1.Id)
                .With(x => x.PendingPaymentId, _pendingPayment1.Id)
                .With(x => x.AccountId, _account1.Id)
                .With(x => x.AccountLegalEntityId, _account1.AccountLegalEntityId)
                .Without(x=>x.PaidDate)
                .Create();

            var sentPayment = _fixture.Build<Payment>()
                .With(x => x.ApprenticeshipIncentiveId, _apprenticeshipIncentive1.Id)
                .With(x => x.PendingPaymentId, _pendingPayment1.Id)
                .With(x => x.AccountId, _account1.Id)
                .With(x => x.AccountLegalEntityId, _account1.AccountLegalEntityId)
                .Create();

            var payments = new List<Payment>()
            {
                _payment1,
                sentPayment
            };

            _context.Payments.AddRange(payments);
            _context.SaveChanges();
        }
    }
}