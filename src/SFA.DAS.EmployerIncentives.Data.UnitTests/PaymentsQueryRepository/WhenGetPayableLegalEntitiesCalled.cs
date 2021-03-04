using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.PaymentsQueryRepository
{
    public class WhenGetPayableLegalEntitiesCalled
    {
        private EmployerIncentivesDbContext _context;
        private Fixture _fixture;
        private IPaymentsQueryRepository _sut;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _context = new EmployerIncentivesDbContext(options);
            _sut = new ApprenticeshipIncentives.PaymentsQueryRepository(new Lazy<EmployerIncentivesDbContext>(_context));
        }

        [TearDown]
        public void CleanUp()
        {
            _context.Dispose();
        }

        [Test]
        public async Task Then_legal_entities_with_pending_payments_in_the_current_period_are_returned()
        {
            short collectionPeriodYear = 2020;
            byte collectionPeriod = 5;
            var pendingPayments = new List<PendingPayment>
            {
                _fixture.Build<PendingPayment>().With(x => x.PaymentYear, collectionPeriodYear).With(x => x.PeriodNumber, collectionPeriod).With(x => x.PaymentMadeDate, (DateTime?)null).Create(),
                _fixture.Build<PendingPayment>().With(x => x.PaymentYear, collectionPeriodYear).With(x => x.PeriodNumber, collectionPeriod).With(x => x.PaymentMadeDate, (DateTime?)null).Create(),
                _fixture.Build<PendingPayment>().With(x => x.PaymentYear, collectionPeriodYear).With(x => x.PeriodNumber, (byte)(collectionPeriod + 1)).With(x => x.PaymentMadeDate, (DateTime?)null).Create(),
            };

            _context.PendingPayments.AddRange(pendingPayments);
            _context.SaveChanges();

            var actual = await _sut.GetPayableLegalEntities(collectionPeriodYear, collectionPeriod);

            actual.Count.Should().Be(2);
            actual.Should().Contain(x => x.AccountLegalEntityId == pendingPayments[0].AccountLegalEntityId);
            actual.Should().Contain(x => x.AccountLegalEntityId == pendingPayments[1].AccountLegalEntityId);
        }

        [Test]
        public async Task Then_legal_entities_with_pending_payments_in_the_previous_year_are_returned()
        {
            short collectionPeriodYear = 2020;
            byte collectionPeriod = 5;
            var pendingPayments = new List<PendingPayment>
            {
                _fixture.Build<PendingPayment>().With(x => x.PaymentYear, collectionPeriodYear).With(x => x.PeriodNumber, collectionPeriod).With(x => x.PaymentMadeDate, (DateTime?)null).Create(),
                _fixture.Build<PendingPayment>().With(x => x.PaymentYear, (short)(collectionPeriodYear - 1)).With(x => x.PeriodNumber, collectionPeriod).With(x => x.PaymentMadeDate, (DateTime?)null).Create(),
                _fixture.Build<PendingPayment>().With(x => x.PaymentYear, collectionPeriodYear).With(x => x.PeriodNumber, (byte)(collectionPeriod + 1)).With(x => x.PaymentMadeDate, (DateTime?)null).Create(),
            };

            _context.PendingPayments.AddRange(pendingPayments);
            _context.SaveChanges();

            var actual = await _sut.GetPayableLegalEntities(collectionPeriodYear, collectionPeriod);

            actual.Count.Should().Be(2);
            actual.Should().Contain(x => x.AccountLegalEntityId == pendingPayments[0].AccountLegalEntityId);
            actual.Should().Contain(x => x.AccountLegalEntityId == pendingPayments[1].AccountLegalEntityId);
        }

        [Test]
        public async Task Then_legal_entities_with_pending_payments_in_a_previous_period_are_returned()
        {
            short collectionPeriodYear = 2020;
            byte collectionPeriod = 5;
            var pendingPayments = new List<PendingPayment>
            {
                _fixture.Build<PendingPayment>().With(x => x.PaymentYear, collectionPeriodYear).With(x => x.PeriodNumber, collectionPeriod).With(x => x.PaymentMadeDate, (DateTime?)null).Create(),
                _fixture.Build<PendingPayment>().With(x => x.PaymentYear, collectionPeriodYear).With(x => x.PeriodNumber, (byte)(collectionPeriod - 1)).With(x => x.PaymentMadeDate, (DateTime?)null).Create(),
                _fixture.Build<PendingPayment>().With(x => x.PaymentYear, collectionPeriodYear).With(x => x.PeriodNumber, (byte)(collectionPeriod + 1)).With(x => x.PaymentMadeDate, (DateTime?)null).Create(),
            };

            _context.PendingPayments.AddRange(pendingPayments);
            _context.SaveChanges();

            var actual = await _sut.GetPayableLegalEntities(collectionPeriodYear, collectionPeriod);

            actual.Count.Should().Be(2);
            actual.Should().Contain(x => x.AccountLegalEntityId == pendingPayments[0].AccountLegalEntityId);
            actual.Should().Contain(x => x.AccountLegalEntityId == pendingPayments[1].AccountLegalEntityId);
        }

        [Test]
        public async Task Then_a_legal_entity_with_multiple_pending_payments_is_only_returned_once()
        {
            short collectionPeriodYear = 2020;
            byte collectionPeriod = 5;
            var pendingPayments = new List<PendingPayment>
            {
                _fixture.Build<PendingPayment>().With(x => x.PaymentYear, collectionPeriodYear).With(x => x.PeriodNumber, collectionPeriod).With(x => x.PaymentMadeDate, (DateTime?)null).With(x => x.AccountLegalEntityId, 1234).With(x=>x.AccountId, 2).Create(),
                _fixture.Build<PendingPayment>().With(x => x.PaymentYear, collectionPeriodYear).With(x => x.PeriodNumber, collectionPeriod).With(x => x.PaymentMadeDate, (DateTime?)null).With(x => x.AccountLegalEntityId, 1234).With(x=>x.AccountId, 2).Create(),
                _fixture.Build<PendingPayment>().With(x => x.PaymentYear, collectionPeriodYear).With(x => x.PeriodNumber, (byte)(collectionPeriod + 1)).With(x => x.PaymentMadeDate, (DateTime?)null).Create(),
            };

            _context.PendingPayments.AddRange(pendingPayments);
            _context.SaveChanges();

            var actual = await _sut.GetPayableLegalEntities(collectionPeriodYear, collectionPeriod);

            actual.Count.Should().Be(1);
            actual.Should().Contain(x => x.AccountLegalEntityId == pendingPayments[0].AccountLegalEntityId);
        }

        [Test]
        public async Task Then_a_legal_entity_where_pending_payments_are_already_made_is_not_returned()
        {
            short collectionPeriodYear = 2020;
            byte collectionPeriod = 5;
            var pendingPayments = new List<PendingPayment>
            {
                _fixture.Build<PendingPayment>().With(x => x.PaymentYear, collectionPeriodYear).With(x => x.PeriodNumber, collectionPeriod).With(x => x.PaymentMadeDate, (DateTime?)null).Create(),
                _fixture.Build<PendingPayment>().With(x => x.PaymentYear, collectionPeriodYear).With(x => x.PeriodNumber, collectionPeriod).With(x => x.PaymentMadeDate, (DateTime?)null).Create(),
                _fixture.Build<PendingPayment>().With(x => x.PaymentYear, collectionPeriodYear).With(x => x.PeriodNumber, collectionPeriod).With(x => x.PaymentMadeDate, DateTime.Now).Create(),
            };

            _context.PendingPayments.AddRange(pendingPayments);
            _context.SaveChanges();

            var actual = await _sut.GetPayableLegalEntities(collectionPeriodYear, collectionPeriod);

            actual.Count.Should().Be(2);
            actual.Should().Contain(x => x.AccountLegalEntityId == pendingPayments[0].AccountLegalEntityId);
            actual.Should().Contain(x => x.AccountLegalEntityId == pendingPayments[1].AccountLegalEntityId);
        }

        [Test]
        public async Task Then_legal_entities_with_unsent_payments_are_returned()
        {
            var payments = new List<Payment>
            {
                _fixture.Build<Payment>().With(x => x.PaidDate, (DateTime?)null).Create(),
                _fixture.Build<Payment>().With(x => x.PaidDate, DateTime.Now).Create(),
                _fixture.Build<Payment>().With(x => x.PaidDate, (DateTime?)null).Create(),
            };

            _context.Payments.AddRange(payments);
            _context.SaveChanges();

            var actual = await _sut.GetPayableLegalEntities(2020, 1);

            actual.Count.Should().Be(2);
            actual.Should().Contain(x => x.AccountLegalEntityId == payments[0].AccountLegalEntityId);
            actual.Should().Contain(x => x.AccountLegalEntityId == payments[2].AccountLegalEntityId);
        }

        [Test]
        public async Task Then_a_legal_entity_with_unsent_payments_and_pending_payments_is_only_returned_once()
        {
            short collectionPeriodYear = 2020;
            byte collectionPeriod = 5;
            var accountLegalEntityId = _fixture.Create<long>();
            var accountId = _fixture.Create<long>();

            var payment = _fixture.Build<Payment>()
                .With(x => x.PaidDate, (DateTime?) null)
                .With(x => x.AccountLegalEntityId, accountLegalEntityId)
                .With(x => x.AccountId, accountId)
                .Create();

            var pendingPayment = _fixture.Build<PendingPayment>()
                .With(x => x.PaymentYear, collectionPeriodYear)
                .With(x => x.PeriodNumber, collectionPeriod)
                .With(x => x.PaymentMadeDate, (DateTime?) null)
                .With(x => x.AccountLegalEntityId, accountLegalEntityId)
                .With(x => x.AccountId, accountId)
                .Create();

            _context.Payments.Add(payment);
            _context.PendingPayments.Add(pendingPayment);
            _context.SaveChanges();

            var actual = await _sut.GetPayableLegalEntities(collectionPeriodYear, collectionPeriod);

            actual.Count.Should().Be(1);
        }
    }
}