using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.PaymentsQueryRepository
{
    public class GetClawbackLegalEntitiesCalled
    {
        private EmployerIncentivesDbContext _context;
        private Fixture _fixture;
        private IPaymentsQueryRepository _sut;
        private Mock<IServiceProvider> _mockServiceProvider;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _mockServiceProvider = new Mock<IServiceProvider>();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _context = new EmployerIncentivesDbContext(options, _mockServiceProvider.Object);
            _sut = new ApprenticeshipIncentives.PaymentsQueryRepository(new Lazy<EmployerIncentivesDbContext>(_context));
        }

        [TearDown]
        public void CleanUp()
        {
            _context.Dispose();
        }

        [Test]
        public async Task Then_legal_entities_with_unsent_clawbacks_in_the_current_period_are_returned_when_isSent_if_false()
        {
            short collectionPeriodYear = 2020;
            byte collectionPeriod = 5;
            var clawbackPayments = new List<ClawbackPayment>
            {
                _fixture.Build<ClawbackPayment>().With(x => x.CollectionPeriodYear, collectionPeriodYear).With(x => x.CollectionPeriod, collectionPeriod).With(x => x.DateClawbackSent, (DateTime?)null).Create(),
                _fixture.Build<ClawbackPayment>().With(x => x.CollectionPeriodYear, collectionPeriodYear).With(x => x.CollectionPeriod, collectionPeriod).With(x => x.DateClawbackSent, (DateTime?)null).Create(),
                _fixture.Build<ClawbackPayment>().With(x => x.CollectionPeriodYear, collectionPeriodYear).With(x => x.CollectionPeriod, collectionPeriod).With(x => x.DateClawbackSent).Create(),
                _fixture.Build<ClawbackPayment>().With(x => x.CollectionPeriodYear, collectionPeriodYear).With(x => x.CollectionPeriod, (byte)(collectionPeriod + 1)).With(x => x.DateClawbackSent, (DateTime?)null).Create(),
            };

            _context.ClawbackPayments.AddRange(clawbackPayments);
            _context.SaveChanges();

            var actual = await _sut.GetClawbackLegalEntities(collectionPeriodYear, collectionPeriod, false);

            actual.Count.Should().Be(2);
            actual.Should().Contain(x => x.AccountLegalEntityId == clawbackPayments[0].AccountLegalEntityId && 
                                    x.AccountId == clawbackPayments[0].AccountId &&
                                    !x.IsSent);
            actual.Should().Contain(x => x.AccountLegalEntityId == clawbackPayments[1].AccountLegalEntityId &&
                                    x.AccountId == clawbackPayments[1].AccountId &&
                                    !x.IsSent);
        }

        [Test]
        public async Task Then_legal_entities_with_sent_clawbacks_in_the_current_period_are_returned_when_isSent_if_true()
        {
            short collectionPeriodYear = 2020;
            byte collectionPeriod = 5;
            var clawbackPayments = new List<ClawbackPayment>
            {
                _fixture.Build<ClawbackPayment>().With(x => x.CollectionPeriodYear, collectionPeriodYear).With(x => x.CollectionPeriod, collectionPeriod).Create(),
                _fixture.Build<ClawbackPayment>().With(x => x.CollectionPeriodYear, collectionPeriodYear).With(x => x.CollectionPeriod, collectionPeriod).Create(),
                _fixture.Build<ClawbackPayment>().With(x => x.CollectionPeriodYear, collectionPeriodYear).With(x => x.CollectionPeriod, collectionPeriod).With(x => x.DateClawbackSent, (DateTime?)null).Create(),
                _fixture.Build<ClawbackPayment>().With(x => x.CollectionPeriodYear, collectionPeriodYear).With(x => x.CollectionPeriod, (byte)(collectionPeriod + 1)).Create(),
            };

            _context.ClawbackPayments.AddRange(clawbackPayments);
            _context.SaveChanges();

            var actual = await _sut.GetClawbackLegalEntities(collectionPeriodYear, collectionPeriod, true);

            actual.Count.Should().Be(2);
            actual.Should().Contain(x => x.AccountLegalEntityId == clawbackPayments[0].AccountLegalEntityId &&
                                    x.AccountId == clawbackPayments[0].AccountId &&
                                    x.IsSent);
            actual.Should().Contain(x => x.AccountLegalEntityId == clawbackPayments[1].AccountLegalEntityId &&
                                    x.AccountId == clawbackPayments[1].AccountId &&
                                    x.IsSent);
        }

        [Test]
        public async Task Then_legal_entities_with_clawbacks_unsent_in_a_previous_period_are_returned()
        {
            short collectionPeriodYear = 2020;
            byte collectionPeriod = 5;

            var clawbackPayments = new List<ClawbackPayment>
            {
                _fixture.Build<ClawbackPayment>().With(x => x.CollectionPeriodYear, collectionPeriodYear).With(x => x.CollectionPeriod, collectionPeriod).With(x => x.DateClawbackSent, (DateTime?)null).Create(),
                _fixture.Build<ClawbackPayment>().With(x => x.CollectionPeriodYear, collectionPeriodYear).With(x => x.CollectionPeriod, (byte)(collectionPeriod - 1)).With(x => x.DateClawbackSent, (DateTime?)null).Create(),
                _fixture.Build<ClawbackPayment>().With(x => x.CollectionPeriodYear, collectionPeriodYear).With(x => x.CollectionPeriod, (byte)(collectionPeriod + 1)).With(x => x.DateClawbackSent, (DateTime?)null).Create(),
            };

            _context.ClawbackPayments.AddRange(clawbackPayments);
            _context.SaveChanges();

            var actual = await _sut.GetClawbackLegalEntities(collectionPeriodYear, collectionPeriod, false);

            actual.Count.Should().Be(2);
            actual.Should().Contain(x => x.AccountLegalEntityId == clawbackPayments[0].AccountLegalEntityId &&
                                    x.AccountId == clawbackPayments[0].AccountId &&
                                    !x.IsSent);
            actual.Should().Contain(x => x.AccountLegalEntityId == clawbackPayments[1].AccountLegalEntityId &&
                                    x.AccountId == clawbackPayments[1].AccountId &&
                                    !x.IsSent);
        }

        [Test]
        public async Task Then_legal_entities_with_clawbacks_sent_in_a_previous_period_are_returned()
        {
            short collectionPeriodYear = 2020;
            byte collectionPeriod = 5;

            var clawbackPayments = new List<ClawbackPayment>
            {
                _fixture.Build<ClawbackPayment>().With(x => x.CollectionPeriodYear, collectionPeriodYear).With(x => x.CollectionPeriod, collectionPeriod).Create(),
                _fixture.Build<ClawbackPayment>().With(x => x.CollectionPeriodYear, collectionPeriodYear).With(x => x.CollectionPeriod, (byte)(collectionPeriod - 1)).Create(),
                _fixture.Build<ClawbackPayment>().With(x => x.CollectionPeriodYear, collectionPeriodYear).With(x => x.CollectionPeriod, (byte)(collectionPeriod + 1)).Create(),
            };

            _context.ClawbackPayments.AddRange(clawbackPayments);
            _context.SaveChanges();

            var actual = await _sut.GetClawbackLegalEntities(collectionPeriodYear, collectionPeriod, true);

            actual.Count.Should().Be(2);
            actual.Should().Contain(x => x.AccountLegalEntityId == clawbackPayments[0].AccountLegalEntityId &&
                                    x.AccountId == clawbackPayments[0].AccountId &&
                                    x.IsSent);
            actual.Should().Contain(x => x.AccountLegalEntityId == clawbackPayments[1].AccountLegalEntityId &&
                                    x.AccountId == clawbackPayments[1].AccountId &&
                                    x.IsSent);
        }

        [Test]
        public async Task Then_a_legal_entity_with_multiple_unsent_clawbacks_is_only_returned_once()
        {
            short collectionPeriodYear = 2020;
            byte collectionPeriod = 5;
                        
            var clawbackPayments = new List<ClawbackPayment>
            {
                _fixture.Build<ClawbackPayment>().With(x => x.CollectionPeriodYear, collectionPeriodYear).With(x => x.CollectionPeriod, collectionPeriod).With(x => x.AccountId, 1234).With(x => x.AccountLegalEntityId, 2).With(x => x.DateClawbackSent, (DateTime?)null).Create(),
                _fixture.Build<ClawbackPayment>().With(x => x.CollectionPeriodYear, collectionPeriodYear).With(x => x.CollectionPeriod, collectionPeriod).With(x => x.AccountId, 1234).With(x => x.AccountLegalEntityId, 2).With(x => x.DateClawbackSent, (DateTime?)null).Create(),
                _fixture.Build<ClawbackPayment>().With(x => x.CollectionPeriodYear, collectionPeriodYear).With(x => x.CollectionPeriod, (byte)(collectionPeriod + 1)).With(x => x.DateClawbackSent, (DateTime?)null).Create()
            };

            _context.ClawbackPayments.AddRange(clawbackPayments);
            _context.SaveChanges();

            var actual = await _sut.GetClawbackLegalEntities(collectionPeriodYear, collectionPeriod, false);

            actual.Count.Should().Be(1);
            actual.Should().Contain(x => x.AccountLegalEntityId == 2 &&
                                    x.AccountId == 1234 &&
                                    !x.IsSent);
        }

        [Test]
        public async Task Then_a_legal_entity_with_multiple_sent_clawbacks_is_only_returned_once()
        {
            short collectionPeriodYear = 2020;
            byte collectionPeriod = 5;

            var clawbackPayments = new List<ClawbackPayment>
            {
                _fixture.Build<ClawbackPayment>().With(x => x.CollectionPeriodYear, collectionPeriodYear).With(x => x.CollectionPeriod, collectionPeriod).With(x => x.AccountId, 1234).With(x => x.AccountLegalEntityId, 2).Create(),
                _fixture.Build<ClawbackPayment>().With(x => x.CollectionPeriodYear, collectionPeriodYear).With(x => x.CollectionPeriod, collectionPeriod).With(x => x.AccountId, 1234).With(x => x.AccountLegalEntityId, 2).Create(),
                _fixture.Build<ClawbackPayment>().With(x => x.CollectionPeriodYear, collectionPeriodYear).With(x => x.CollectionPeriod, (byte)(collectionPeriod + 1)).Create()
            };

            _context.ClawbackPayments.AddRange(clawbackPayments);
            _context.SaveChanges();

            var actual = await _sut.GetClawbackLegalEntities(collectionPeriodYear, collectionPeriod, true);

            actual.Count.Should().Be(1);
            actual.Should().Contain(x => x.AccountLegalEntityId == 2 &&
                                    x.AccountId == 1234 &&
                                    x.IsSent);
        }
    }
}