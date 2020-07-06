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
    public class WhenFindCalled
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
            var testAccount = _fixture.Create<Models.Account>();
            _dbContext.Add(testAccount);
            _dbContext.SaveChanges();

            // Act
            var account = await _sut.Find(testAccount.Id);

            // Assert
            account.Should().NotBeNull();
            account.Id.Should().Be(testAccount.Id);
            var legalEntity = account.LegalEntityModels.Single();
            legalEntity.Id.Should().Be(testAccount.LegalEntityId);
            legalEntity.Name.Should().Be(testAccount.LegalEntityName);
            legalEntity.AccountLegalEntityId.Should().Be(testAccount.AccountLegalEntityId);
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
