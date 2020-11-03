using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Data.Models;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.IncentiveApplicationQueryRepository
{
    public class WhenGetIncentiveApplicationsCalled
    {
        private ApplicationDbContext _context;
        private Fixture _fixture;
        private IQueryRepository<IncentiveApplicationDto> _sut;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("ApplicationDbContext" + Guid.NewGuid()).Options;
            _context = new ApplicationDbContext(options);

            _sut = new IncentiveApplication.IncentiveApplicationQueryRepository(_context);
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
            var account = _fixture.Create<Models.Account>();
            var allApplications = _fixture.Build<Models.IncentiveApplication>().With(x => x.AccountLegalEntityId, account.AccountLegalEntityId).CreateMany<Models.IncentiveApplication>(10).ToArray();
            const long accountId = -1;

            allApplications[0].AccountId = accountId;
            allApplications[3].AccountId = accountId;

            _context.Accounts.Add(account);
            _context.Applications.AddRange(allApplications);
            _context.SaveChanges();

            // Act
            var actual = (await _sut.GetList(x => x.AccountId == accountId)).ToArray();

            //Assert
            actual.All(x => x.AccountId == accountId).Should().BeTrue();
            actual.Should().BeEquivalentTo(new[] {allApplications[0], allApplications[3]}, opts => opts.ExcludingMissingMembers());
        }
    }
}