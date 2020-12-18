using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Data.IncentiveApplicationApprenticeship;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.Accounts;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.IncentiveApplicationApprenticeship
{
    public class WhenGetIncentiveApplicationApprenticeshipCalled
    {
        private EmployerIncentivesDbContext _context;
        private Fixture _fixture;
        private IQueryRepository<Models.IncentiveApplicationApprenticeship> _sut;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _context = new EmployerIncentivesDbContext(options);

            _sut = new IncentiveApplicationApprenticeshipQueryRepository(new Lazy<EmployerIncentivesDbContext>(_context));
        }

        [TearDown]
        public void CleanUp()
        {
            _context.Dispose();
        }

        [Test]
        public async Task Then_data_is_fetched_from_database()
        {
            var allApprenticeships = _fixture.Build<Models.IncentiveApplicationApprenticeship>().CreateMany(10).ToArray();
            var apprenticeshipId = Guid.NewGuid();

            allApprenticeships[1].Id = apprenticeshipId;

            _context.ApplicationApprenticeships.AddRange(allApprenticeships);
            _context.SaveChanges();

            // Act
            var actual = await _sut.Get(x => x.Id == apprenticeshipId);

            //Assert
            actual.Should().BeEquivalentTo(allApprenticeships[1], opts => opts.ExcludingMissingMembers());
        }

        [Test]
        public async Task Then_null_is_returned_when_the_application_is_not_found()
        {
            var allApprenticeships = _fixture.Build<Models.IncentiveApplicationApprenticeship>().CreateMany(10).ToArray();
            var apprenticeshipId = Guid.NewGuid();

            _context.ApplicationApprenticeships.AddRange(allApprenticeships);
            _context.SaveChanges();

            // Act
            var actual = await _sut.Get(x => x.Id == apprenticeshipId);

            //Assert
            actual.Should().BeNull();
        }
    }
}