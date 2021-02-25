using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.IncentiveApplicationApprenticeship;
using SFA.DAS.EmployerIncentives.Data.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.IncentiveApplicationApprenticeship
{
    public class WhenGetListIsCalled
    {
        private EmployerIncentivesDbContext _context;
        private readonly Fixture _fixture = new Fixture();
        private IQueryRepository<Models.IncentiveApplicationApprenticeship> _sut;

        [SetUp]
        public void Arrange()
        {
            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _context = new EmployerIncentivesDbContext(options);

            _sut = new IncentiveApplicationApprenticeshipQueryRepository(new Lazy<EmployerIncentivesDbContext>(_context));
        }

        [TearDown]
        public void CleanUp() => _context.Dispose();

        [Test]
        public async Task Then_data_is_fetched_from_database()
        {
            // Arrange
            var apprenticeships = _fixture.Build<Models.IncentiveApplicationApprenticeship>().CreateMany(10).ToArray();
            apprenticeships[0].FirstName = "Jon";
            apprenticeships[1].FirstName = "Jonathan";
            apprenticeships[2].FirstName = "Jonny";
            apprenticeships[3].FirstName = "Jon Paul";

            var expected = new[] { apprenticeships[0], apprenticeships[1], apprenticeships[2], apprenticeships[3] };

            await _context.ApplicationApprenticeships.AddRangeAsync(apprenticeships);
            await _context.SaveChangesAsync();

            // Act
            var actual = await _sut.GetList(x => x.FirstName.StartsWith("Jon"));

            // Assert
            actual.Should().BeEquivalentTo(expected, opts => opts.ExcludingMissingMembers());
        }

    }
}