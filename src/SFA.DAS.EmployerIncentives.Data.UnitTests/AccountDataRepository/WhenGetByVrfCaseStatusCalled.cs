using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.AccountDataRepository
{
    [TestFixture]
    public class WhenGetByVrfCaseStatusCalled
    {
        private EmployerIncentivesDbContext _context;
        private Fixture _fixture;
        private IAccountDataRepository _sut;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _context = new EmployerIncentivesDbContext(options);

            _sut = new Data.AccountDataRepository(new Lazy<EmployerIncentivesDbContext>(_context));
        }

        [TearDown]
        public void CleanUp()
        {
            _context.Dispose();
        }

        [TestCase(LegalEntityVrfCaseStatus.Completed)]
        [TestCase(LegalEntityVrfCaseStatus.RejectedDataValidation)]
        [TestCase(LegalEntityVrfCaseStatus.RejectedVer1)]
        [TestCase(LegalEntityVrfCaseStatus.RejectedVerification)]
        [TestCase("")]
        [TestCase(null)]
        public async Task Then_data_is_fetched_from_database_for_an_account_with_a_single_legal_entity(string vrfCaseStatus)
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            const long accountId = -1;

            allAccounts[0].Id = accountId;
            allAccounts[0].VrfCaseStatus = vrfCaseStatus;

            _context.Accounts.AddRange(allAccounts);

            var allApplications = _fixture.CreateMany<Models.IncentiveApplication>(10).ToArray();
            for(var i = 0; i < 10; i++)
            {
                allApplications[i].AccountId = allAccounts[i].Id;
                allApplications[i].AccountLegalEntityId = allAccounts[i].AccountLegalEntityId;
            }
            _context.Applications.AddRange(allApplications);

            _context.SaveChanges();

            // Act
            var actual = (await _sut.GetByVrfCaseStatus(vrfCaseStatus)).ToArray();

            //Assert
            actual.All(x => x.AccountId == accountId).Should().BeTrue();
            actual.Length.Should().Be(1);
            actual[0].LegalEntities.Count(x => x.AccountLegalEntityId == allAccounts[0].AccountLegalEntityId).Should().Be(1);
        }

        [TestCase(LegalEntityVrfCaseStatus.Completed)]
        [TestCase(LegalEntityVrfCaseStatus.RejectedDataValidation)]
        [TestCase(LegalEntityVrfCaseStatus.RejectedVer1)]
        [TestCase(LegalEntityVrfCaseStatus.RejectedVerification)]
        [TestCase("")]
        [TestCase(null)]
        public async Task Then_data_is_fetched_from_database_for_an_account_with_a_multiple_legal_entities(string vrfCaseStatus)
        {
            // Arrange
            var allAccounts = _fixture.CreateMany<Models.Account>(10).ToArray();
            const long accountId = -1;

            allAccounts[0].Id = accountId;
            allAccounts[0].VrfCaseStatus = vrfCaseStatus;
            allAccounts[1].Id = accountId;
            allAccounts[1].VrfCaseStatus = vrfCaseStatus;

            _context.Accounts.AddRange(allAccounts);

            var allApplications = _fixture.CreateMany<Models.IncentiveApplication>(10).ToArray();
            for (var i = 0; i < 10; i++)
            {
                allApplications[i].AccountId = allAccounts[i].Id;
                allApplications[i].AccountLegalEntityId = allAccounts[i].AccountLegalEntityId;
            }
            _context.Applications.AddRange(allApplications);

            _context.SaveChanges();

            // Act
            var actual = (await _sut.GetByVrfCaseStatus(vrfCaseStatus)).ToArray();

            //Assert
            actual.All(x => x.AccountId == accountId).Should().BeTrue();
            actual.Length.Should().Be(1);
            actual[0].LegalEntities.FirstOrDefault(x => x.AccountLegalEntityId == allAccounts[0].AccountLegalEntityId).Should().NotBeNull();
            actual[0].LegalEntities.FirstOrDefault(x => x.AccountLegalEntityId == allAccounts[1].AccountLegalEntityId).Should().NotBeNull();
        }
    }
}
