using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
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
        private Mock<IServiceProvider> _mockServiceProvider;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _mockServiceProvider = new Mock<IServiceProvider>();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options, _mockServiceProvider.Object);

            _sut = new Data.AccountDataRepository(new Lazy<EmployerIncentivesDbContext>(_dbContext));
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
        public async Task Then_when_the_account_has_multiple_legal_entities_all_legal_entities_are_returned()
        {
            var accountId = _fixture.Create<long>();
            var testAccounts = _fixture.Build<Models.Account>().With(x => x.Id, accountId).CreateMany(5);
            
            await _dbContext.AddRangeAsync(testAccounts);
            await _dbContext.SaveChangesAsync();

            // Act
            var accounts = await _sut.GetByHashedLegalEntityId(testAccounts.First().HashedLegalEntityId);

            // Assert
            accounts.Single().LegalEntityModels.Count.Should().Be(5);
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
