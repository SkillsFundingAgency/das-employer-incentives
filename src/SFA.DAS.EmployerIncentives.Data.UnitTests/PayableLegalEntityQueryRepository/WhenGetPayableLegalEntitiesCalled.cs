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
    public class WhenGetPayableLegalEntitiesCalled
    {
        private EmployerIncentivesDbContext _context;
        private Fixture _fixture;
        private IPayableLegalEntityQueryRepository _sut;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _context = new EmployerIncentivesDbContext(options);

            _sut = new ApprenticeshipIncentives.PayableLegalEntityQueryRepository(_context);
        }

        [TearDown]
        public void CleanUp()
        {
            _context.Dispose();
        }

        [Test]
        public async Task Then_legal_entities_with_payments_in_the_current_period_are_returned()
        {
            short collectionPeriodYear = 2020;
            byte collectionPeriodMonth = 5;
            var pendingPayments = new List<PendingPayment>
            {
                _fixture.Build<PendingPayment>().With(x => x.PaymentYear, collectionPeriodYear).With(x => x.PeriodNumber, collectionPeriodMonth).With(x => x.PaymentMadeDate, (DateTime?)null).Create(),
                _fixture.Build<PendingPayment>().With(x => x.PaymentYear, collectionPeriodYear).With(x => x.PeriodNumber, collectionPeriodMonth).With(x => x.PaymentMadeDate, (DateTime?)null).Create(),
                _fixture.Build<PendingPayment>().With(x => x.PaymentYear, collectionPeriodYear).With(x => x.PeriodNumber, (byte)(collectionPeriodMonth + 1)).With(x => x.PaymentMadeDate, (DateTime?)null).Create(),
            };
            
            _context.PendingPayments.AddRange(pendingPayments);
            _context.SaveChanges();

            var actual = await _sut.GetList(collectionPeriodYear, collectionPeriodMonth);

            actual.Count.Should().Be(2);
            actual.Should().Contain(x => x.AccountLegalEntityId == pendingPayments[0].AccountLegalEntityId);
            actual.Should().Contain(x => x.AccountLegalEntityId == pendingPayments[1].AccountLegalEntityId);
        }

        [Test]
        public async Task Then_legal_entities_with_payments_in_the_previous_year_are_returned()
        {
            short collectionPeriodYear = 2020;
            byte collectionPeriodMonth = 5;
            var pendingPayments = new List<PendingPayment>
            {
                _fixture.Build<PendingPayment>().With(x => x.PaymentYear, collectionPeriodYear).With(x => x.PeriodNumber, collectionPeriodMonth).With(x => x.PaymentMadeDate, (DateTime?)null).Create(),
                _fixture.Build<PendingPayment>().With(x => x.PaymentYear, (short)(collectionPeriodYear - 1)).With(x => x.PeriodNumber, collectionPeriodMonth).With(x => x.PaymentMadeDate, (DateTime?)null).Create(),
                _fixture.Build<PendingPayment>().With(x => x.PaymentYear, collectionPeriodYear).With(x => x.PeriodNumber, (byte)(collectionPeriodMonth + 1)).With(x => x.PaymentMadeDate, (DateTime?)null).Create(),
            };

            _context.PendingPayments.AddRange(pendingPayments);
            _context.SaveChanges();

            var actual = await _sut.GetList(collectionPeriodYear, collectionPeriodMonth);

            actual.Count.Should().Be(2);
            actual.Should().Contain(x => x.AccountLegalEntityId == pendingPayments[0].AccountLegalEntityId);
            actual.Should().Contain(x => x.AccountLegalEntityId == pendingPayments[1].AccountLegalEntityId);
        }

        [Test]
        public async Task Then_legal_entities_with_payments_in_a_previous_period_are_returned()
        {
            short collectionPeriodYear = 2020;
            byte collectionPeriodMonth = 5;
            var pendingPayments = new List<PendingPayment>
            {
                _fixture.Build<PendingPayment>().With(x => x.PaymentYear, collectionPeriodYear).With(x => x.PeriodNumber, collectionPeriodMonth).With(x => x.PaymentMadeDate, (DateTime?)null).Create(),
                _fixture.Build<PendingPayment>().With(x => x.PaymentYear, collectionPeriodYear).With(x => x.PeriodNumber, (byte)(collectionPeriodMonth - 1)).With(x => x.PaymentMadeDate, (DateTime?)null).Create(),
                _fixture.Build<PendingPayment>().With(x => x.PaymentYear, collectionPeriodYear).With(x => x.PeriodNumber, (byte)(collectionPeriodMonth + 1)).With(x => x.PaymentMadeDate, (DateTime?)null).Create(),
            };

            _context.PendingPayments.AddRange(pendingPayments);
            _context.SaveChanges();

            var actual = await _sut.GetList(collectionPeriodYear, collectionPeriodMonth);

            actual.Count.Should().Be(2);
            actual.Should().Contain(x => x.AccountLegalEntityId == pendingPayments[0].AccountLegalEntityId);
            actual.Should().Contain(x => x.AccountLegalEntityId == pendingPayments[1].AccountLegalEntityId);
        }

        [Test]
        public async Task Then_a_legal_entity_with_multiple_payments_is_only_returned_once()
        {
            short collectionPeriodYear = 2020;
            byte collectionPeriodMonth = 5;
            var pendingPayments = new List<PendingPayment>
            {
                _fixture.Build<PendingPayment>().With(x => x.PaymentYear, collectionPeriodYear).With(x => x.PeriodNumber, collectionPeriodMonth).With(x => x.PaymentMadeDate, (DateTime?)null).With(x => x.AccountLegalEntityId, 1234).With(x=>x.AccountId, 2).Create(),
                _fixture.Build<PendingPayment>().With(x => x.PaymentYear, collectionPeriodYear).With(x => x.PeriodNumber, collectionPeriodMonth).With(x => x.PaymentMadeDate, (DateTime?)null).With(x => x.AccountLegalEntityId, 1234).With(x=>x.AccountId, 2).Create(),
                _fixture.Build<PendingPayment>().With(x => x.PaymentYear, collectionPeriodYear).With(x => x.PeriodNumber, (byte)(collectionPeriodMonth + 1)).With(x => x.PaymentMadeDate, (DateTime?)null).Create(),
            };

            _context.PendingPayments.AddRange(pendingPayments);
            _context.SaveChanges();

            var actual = await _sut.GetList(collectionPeriodYear, collectionPeriodMonth);

            actual.Count.Should().Be(1);
            actual.Should().Contain(x => x.AccountLegalEntityId == pendingPayments[0].AccountLegalEntityId);
        }

        [Test]
        public async Task Then_a_legal_entity_where_payments_are_already_made_is_not_returned()
        {
            short collectionPeriodYear = 2020;
            byte collectionPeriodMonth = 5;
            var pendingPayments = new List<PendingPayment>
            {
                _fixture.Build<PendingPayment>().With(x => x.PaymentYear, collectionPeriodYear).With(x => x.PeriodNumber, collectionPeriodMonth).With(x => x.PaymentMadeDate, (DateTime?)null).Create(),
                _fixture.Build<PendingPayment>().With(x => x.PaymentYear, collectionPeriodYear).With(x => x.PeriodNumber, collectionPeriodMonth).With(x => x.PaymentMadeDate, (DateTime?)null).Create(),
                _fixture.Build<PendingPayment>().With(x => x.PaymentYear, collectionPeriodYear).With(x => x.PeriodNumber, collectionPeriodMonth).With(x => x.PaymentMadeDate, DateTime.Now).Create(),
            };

            _context.PendingPayments.AddRange(pendingPayments);
            _context.SaveChanges();

            var actual = await _sut.GetList(collectionPeriodYear, collectionPeriodMonth);

            actual.Count.Should().Be(2);
            actual.Should().Contain(x => x.AccountLegalEntityId == pendingPayments[0].AccountLegalEntityId);
            actual.Should().Contain(x => x.AccountLegalEntityId == pendingPayments[1].AccountLegalEntityId);
        }
    }
}