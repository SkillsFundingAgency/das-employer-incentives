using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.ApprenticeApplicationDataRepository
{
    [TestFixture]
    public class WhenGetListCalled
    {
        private EmployerIncentivesDbContext _context;
        private Fixture _fixture;
        private Data.ApprenticeApplicationDataRepository _sut;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _context = new EmployerIncentivesDbContext(options);

            _sut = new Data.ApprenticeApplicationDataRepository(new Lazy<EmployerIncentivesDbContext>(_context));
        }

        [TearDown]
        public void CleanUp()
        {
            _context.Dispose();
        }

        [Test]
        public async Task Then_data_is_fetched_from_the_database()
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            var accountId = _fixture.Create<long>();
            var accountLegalEntityId = _fixture.Create<long>();

            allAccounts[0].Id = accountId;
            allAccounts[0].AccountLegalEntityId = accountLegalEntityId;

            var allApplications = _fixture.CreateMany<Models.IncentiveApplication>(5).ToArray();
            allApplications[0].AccountId = accountId;
            allApplications[0].AccountLegalEntityId = accountLegalEntityId;

            var allApprenticeships = _fixture.CreateMany<Models.IncentiveApplicationApprenticeship>(10).ToArray();
            allApprenticeships[1].IncentiveApplicationId = allApplications[0].Id;
            allApprenticeships[2].IncentiveApplicationId = allApplications[0].Id;
            allApprenticeships[3].IncentiveApplicationId = allApplications[0].Id;
            allApprenticeships[4].IncentiveApplicationId = allApplications[0].Id;
            allApprenticeships[5].IncentiveApplicationId = allApplications[0].Id;

            _context.Accounts.AddRange(allAccounts);
            _context.Applications.AddRange(allApplications);
            _context.ApplicationApprenticeships.AddRange(allApprenticeships);

            _context.SaveChanges();

            // Act
            var result = (await _sut.GetList(accountId, accountLegalEntityId)).ToArray();

            // Assert
            result.All(x => x.AccountId == accountId).Should().BeTrue();
        }
    }
}
