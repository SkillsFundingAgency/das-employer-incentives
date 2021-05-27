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
    public class WhenGetApprenticeshipIncentivesIsCalled
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
        public async Task Then_all_apprenticeship_incentives_are_returned()
        {
            var apprenticeshipIncentives =
                _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>().ToList();
            foreach (var incentive in apprenticeshipIncentives)
            {
                incentive.Status = IncentiveStatus.Active;
            }

            _context.ApprenticeshipIncentives.AddRange(apprenticeshipIncentives);
            _context.SaveChanges();

            var actual = await _sut.GetList();

            actual.Count.Should().Be(apprenticeshipIncentives.Count);
            actual.Should().Contain(x => x.Id == apprenticeshipIncentives[0].Id);
            actual.Should().Contain(x => x.ApprenticeshipId == apprenticeshipIncentives[0].ApprenticeshipId);
            actual.Should().Contain(x => x.ULN == apprenticeshipIncentives[0].ULN);
            actual.Should().Contain(x => x.UKPRN == apprenticeshipIncentives[0].UKPRN);
        }

        [Test]
        public async Task Then_withdrawn_apprenticeship_incentives_are_excluded()
        {
            var apprenticeshipIncentives =
                _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>(10).ToList();

            foreach (var incentive in apprenticeshipIncentives)
            {
                incentive.Status = IncentiveStatus.Active;
            }
            apprenticeshipIncentives[0].Status = IncentiveStatus.Withdrawn;

            _context.ApprenticeshipIncentives.AddRange(apprenticeshipIncentives);
            _context.SaveChanges();

            var actual = await _sut.GetList();

            actual.Count.Should().Be(apprenticeshipIncentives.Count - 1);
            actual.Should().NotContain(x => x.ULN == apprenticeshipIncentives[0].ULN);
        }
    }
}