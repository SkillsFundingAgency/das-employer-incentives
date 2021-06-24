using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.ApprenticeshipIncentiveQueryRepository
{
    [TestFixture]
    public class WhenGetWithdrawableIsCalled
    {
        private EmployerIncentivesDbContext _context;
        private Fixture _fixture;
        private ApprenticeshipIncentives.ApprenticeshipIncentiveQueryRepository _sut;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _context = new EmployerIncentivesDbContext(options);

            _sut = new ApprenticeshipIncentives.ApprenticeshipIncentiveQueryRepository(new Lazy<EmployerIncentivesDbContext>(_context));
        }

        [TearDown]
        public void CleanUp()
        {
            _context.Dispose();
        }

        [Test]
        public async Task Then_all_apprenticeship_incentives_for_the_account_legal_entity_are_returned()
        {
            var accountId = 12345;
            var accountLegalEntityId = 67890;
            var apprenticeshipIncentives =
                _fixture.Build<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>()
                    .With(x => x.Status, IncentiveStatus.Active)
                    .With(x => x.AccountId, accountId)
                    .With(x => x.AccountLegalEntityId, accountLegalEntityId).CreateMany().ToList();

            _context.ApprenticeshipIncentives.AddRange(apprenticeshipIncentives);
            _context.SaveChanges();

            var actual = await _sut.GetWithdrawable(accountId, accountLegalEntityId);

            actual.Count.Should().Be(apprenticeshipIncentives.Count);
            actual.Should().Contain(x => x.Id == apprenticeshipIncentives[0].Id);
            actual.Should().Contain(x => x.ApprenticeshipId == apprenticeshipIncentives[0].ApprenticeshipId);
            actual.Should().Contain(x => x.ULN == apprenticeshipIncentives[0].ULN);
            actual.Should().Contain(x => x.UKPRN == apprenticeshipIncentives[0].UKPRN);
            actual.Should().Contain(x => x.FirstName == apprenticeshipIncentives[0].FirstName);
            actual.Should().Contain(x => x.LastName == apprenticeshipIncentives[0].LastName);
            actual.Should().Contain(x => x.CourseName == apprenticeshipIncentives[0].CourseName);
            actual.Should().Contain(x => x.StartDate == apprenticeshipIncentives[0].StartDate);
        }

        [Test]
        public async Task Then_withdrawn_apprenticeship_incentives_are_excluded()
        {
            var accountId = 12345;
            var accountLegalEntityId = 67890;
            var apprenticeshipIncentives =
                _fixture.Build<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>()
                    .With(x => x.Status, IncentiveStatus.Active)
                    .With(x => x.AccountId, accountId)
                    .With(x => x.AccountLegalEntityId, accountLegalEntityId).CreateMany().ToList();

            apprenticeshipIncentives[0].Status = IncentiveStatus.Withdrawn;

            _context.ApprenticeshipIncentives.AddRange(apprenticeshipIncentives);
            _context.SaveChanges();

            var actual = await _sut.GetWithdrawable(accountId, accountLegalEntityId);

            actual.Count.Should().Be(apprenticeshipIncentives.Count - 1);
            actual.Should().NotContain(x => x.ULN == apprenticeshipIncentives[0].ULN);
        }

        [Test]
        public async Task Then_apprenticeship_incentives_for_a_different_account_are_excluded()
        {
            var accountId = 12345;
            var accountLegalEntityId = 67890;
            var apprenticeshipIncentives =
                _fixture.Build<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>()
                    .With(x => x.Status, IncentiveStatus.Active)
                    .With(x => x.AccountId, accountId)
                    .With(x => x.AccountLegalEntityId, accountLegalEntityId).CreateMany().ToList();

            apprenticeshipIncentives[0].AccountId = accountId + 1;

            _context.ApprenticeshipIncentives.AddRange(apprenticeshipIncentives);
            _context.SaveChanges();

            var actual = await _sut.GetWithdrawable(accountId, accountLegalEntityId);

            actual.Count.Should().Be(apprenticeshipIncentives.Count - 1);
            actual.Should().NotContain(x => x.ULN == apprenticeshipIncentives[0].ULN);
        }

        [Test]
        public async Task Then_apprenticeship_incentives_for_a_different_account_legal_entity_are_excluded()
        {
            var accountId = 12345;
            var accountLegalEntityId = 67890;
            var apprenticeshipIncentives =
                _fixture.Build<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>()
                    .With(x => x.Status, IncentiveStatus.Active)
                    .With(x => x.AccountId, accountId)
                    .With(x => x.AccountLegalEntityId, accountLegalEntityId).CreateMany().ToList();

            apprenticeshipIncentives[0].AccountLegalEntityId = accountLegalEntityId + 1;

            _context.ApprenticeshipIncentives.AddRange(apprenticeshipIncentives);
            _context.SaveChanges();

            var actual = await _sut.GetWithdrawable(accountId, accountLegalEntityId);

            actual.Count.Should().Be(apprenticeshipIncentives.Count - 1);
            actual.Should().NotContain(x => x.ULN == apprenticeshipIncentives[0].ULN);
        }
    }
}