using AutoFixture;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Account;
using SFA.DAS.EmployerIncentives.Data.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.DataTransferObjects;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.AccountDataRepository
{
    public class WhenGetLegalEntityCalled
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

            _sut = new AccountQueryRepository(new Lazy<EmployerIncentivesDbContext>(_context));
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
            const long accountLegalEntityId = -1;

            allAccounts[1].AccountLegalEntityId = accountLegalEntityId;
            
            _context.Accounts.AddRange(allAccounts);
            _context.SaveChanges();

            // Act
            var actual = await _sut.Get(x => x.AccountLegalEntityId == accountLegalEntityId);

            //Assert
            actual.Should().BeEquivalentTo(allAccounts[1], opts => opts.ExcludingMissingMembers());
        }
    }
}