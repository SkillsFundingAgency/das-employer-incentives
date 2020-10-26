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
    public class WhenGetByHashedLegalEntityId
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

            _sut = new Data.AccountDataRepository(_dbContext);
        }

        [TearDown]
        public void CleanUp()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task Then_the_expected_account_is_returned_if_it_exists()
        {
            // Arrange
            const string legalEntityId = "XYZ123__";
            var testAccounts = _fixture.CreateMany<Models.Account>(4).ToList();
            testAccounts.First().HashedLegalEntityId = legalEntityId;
            testAccounts.Last().HashedLegalEntityId = legalEntityId;

            _dbContext.AddRange(testAccounts);
            _dbContext.SaveChanges();

            // Act
            var accounts = await _sut.GetByHashedLegalEntityId(legalEntityId);

            // Assert
            accounts.Should().BeEquivalentTo(testAccounts.Where(x => x.HashedLegalEntityId == legalEntityId),
                opt => opt.ExcludingMissingMembers());
        }

        [Test]
        public async Task Then_a_null_account_is_returned_if_it_does_not_exist()
        {
            // Arrange
            var testAccountId = _fixture.Create<long>();

            // Act
            var account = await _sut.Find(testAccountId);

            // Assert
            account.Should().BeNull();
        }
    }
}
