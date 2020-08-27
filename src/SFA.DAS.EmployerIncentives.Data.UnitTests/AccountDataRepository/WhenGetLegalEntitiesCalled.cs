using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions;
using SFA.DAS.EmployerIncentives.Data.Account;
using SFA.DAS.EmployerIncentives.Data.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.AccountDataRepository
{
    public class WhenGetLegalEntitiesCalled
    {
        private EmployerIncentivesDbContext _context;
        private Fixture _fixture;
        private IQueryRepository<LegalEntityDto> _sut;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _context = new EmployerIncentivesDbContext(options);

            _sut = new AccountQueryRepository(_context);
        }

        [TearDown]
        public void CleanUp()
        {
            _context.Dispose();
        }

        [Test]
        public async Task Then_data_is_fetched_from_database()
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            const long accountId = -1;

            allAccounts[0].Id = accountId;
            allAccounts[1].Id = accountId;

            _context.Accounts.AddRange(allAccounts);
            _context.SaveChanges();

            // Act
            var actual = (await _sut.GetList(x => x.AccountId == accountId)).ToArray();

            //Assert
            actual.All(x => x.AccountId == accountId).Should().BeTrue();
            actual.Should().BeEquivalentTo(new [] {allAccounts[0], allAccounts[1]},
                opts => opts.ExcludingMissingMembers());
        }
    }
}