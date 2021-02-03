using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.AccountDataRepository
{
    public class WhenGetLatestVendorRegistrationCaseUpdateDateTime
    {
        private Data.AccountDataRepository _sut;
        private Fixture _fixture;
        private EmployerIncentivesDbContext _dbContext;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;

            _dbContext = new EmployerIncentivesDbContext(options);

            _sut = new Data.AccountDataRepository(new Lazy<EmployerIncentivesDbContext>(_dbContext));
        }

        [TearDown]
        public void CleanUp()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task Then_the_max_vrf_update_date_time_is_returned_from_database()
        {
            // Arrange
            var testAccounts = _fixture.CreateMany<Models.Account>(100).ToList();
            testAccounts[25].VrfCaseStatusLastUpdatedDateTime = null;
            testAccounts[77].VrfCaseStatusLastUpdatedDateTime = null;

            await _dbContext.AddRangeAsync(testAccounts);
            await _dbContext.SaveChangesAsync();

            // ReSharper disable once PossibleInvalidOperationException
            var expected = testAccounts.OrderByDescending(x => x.VrfCaseStatusLastUpdatedDateTime).First()
                .VrfCaseStatusLastUpdatedDateTime.Value;

            // Act
            var result = await _sut.GetLatestVendorRegistrationCaseUpdateDateTime();

            // Assert
            result.Should().Be(expected);
        }

        [Test]
        public async Task Then_null_is_returned_when_no_vrf_update_date_time_is_in_the_database()
        {
            // Arrange
            var testAccounts = _fixture.Build<Models.Account>()
                .Without(x => x.VrfCaseStatusLastUpdatedDateTime)
                .CreateMany(300).ToList();

            await _dbContext.AddRangeAsync(testAccounts);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _sut.GetLatestVendorRegistrationCaseUpdateDateTime();

            // Assert
            result.Should().BeNull();
        }
    }
}