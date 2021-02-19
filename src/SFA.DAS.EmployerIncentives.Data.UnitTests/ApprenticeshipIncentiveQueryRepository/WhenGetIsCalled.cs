using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.ApprenticeshipIncentiveQueryRepository
{
    public class WhenGetIsCalled
    {
        private EmployerIncentivesDbContext _context;
        private readonly Fixture _fixture = new Fixture();
        private ApprenticeshipIncentives.ApprenticeshipIncentiveQueryRepository _sut;

        [SetUp]
        public void Arrange()
        {
            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _context = new EmployerIncentivesDbContext(options);
            _sut = new ApprenticeshipIncentives.ApprenticeshipIncentiveQueryRepository(new Lazy<EmployerIncentivesDbContext>(_context));
        }

        [TearDown]
        public void CleanUp() => _context.Dispose();

        [Test]
        public async Task Then_a_single_apprenticeship_incentives_is_returned_using_a_filter()
        {
            // Arrange
            var apprenticeshipIncentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>().ToList();

            // Act
            await _context.AddRangeAsync(apprenticeshipIncentives);
            await _context.SaveChangesAsync();

            // Assert
            var actual = await _sut.Get(x => x.Id == apprenticeshipIncentives[0].Id);
            actual.Should().BeEquivalentTo(apprenticeshipIncentives[0]);
        }

        [Test]
        public async Task Then_all_apprenticeship_incentives_are_returned_using_a_filter()
        {
            // Arrange
            var apprenticeshipIncentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>().ToList();
            apprenticeshipIncentives[0].CourseName = "TEST 123";

            // Act
            await _context.AddRangeAsync(apprenticeshipIncentives);
            await _context.SaveChangesAsync();

            // Assert
            var actual = await _sut.GetList(x => x.CourseName.StartsWith("TEST "));
            actual.Should().BeEquivalentTo(apprenticeshipIncentives[0]);
        }

        [Test]
        public async Task Then_a_single_apprenticeship_incentives_is_returned_using_a_filter_and_including_payments()
        {
            // Arrange
            var apprenticeshipIncentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>().ToList();

            // Act
            await _context.AddRangeAsync(apprenticeshipIncentives);
            await _context.SaveChangesAsync();

            // Assert
            var actual = await _sut.Get(x => x.Id == apprenticeshipIncentives[0].Id, true);
            actual.Should().BeEquivalentTo(apprenticeshipIncentives[0]);
            actual.Payments.Should().HaveCount(3);
        }

        [Test]
        public async Task Then_a_single_apprenticeship_incentives_is_returned_using_a_filter_and_excluding_payments()
        {
            // Arrange
            var apprenticeshipIncentives = _fixture.CreateMany<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>().ToList();

            // Act
            await _context.AddRangeAsync(apprenticeshipIncentives);
            await _context.SaveChangesAsync();

            // Assert
            var actual = await _sut.Get(x => x.Id == apprenticeshipIncentives[0].Id, false);
            actual.Should().BeEquivalentTo(apprenticeshipIncentives[0]);
        }
    }
}