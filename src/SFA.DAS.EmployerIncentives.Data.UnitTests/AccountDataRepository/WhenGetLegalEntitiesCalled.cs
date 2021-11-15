using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
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


        [Test]
        [TestCase(null,false)]
        [TestCase(1,false)]
        [TestCase(2,false)]
        [TestCase(3,false)]
        [TestCase(4,false)]
        [TestCase(5,false)]
        [TestCase(6,true)]
        [TestCase(7,true)]
        public async Task Then_has_signed_agreement_version_is_set(int? signedAgreementVersion, bool expected)
        {
            // Arrange
            var account = _fixture.Create<Models.Account>();
            account.SignedAgreementVersion = signedAgreementVersion;

            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();

            // Act
            var actual = (await _sut.GetList(x => x.AccountId == account.Id)).Single();

            // Assert
            actual.IsAgreementSigned.Should().Be(expected);
        }

        [TestCase(null, true)]
        [TestCase("", true)]
        [TestCase(" ", true)]
        [TestCase("Requested", false)]
        [TestCase("Case request completed", false)]
        public async Task Then_bank_details_required_is_set(string vrfCaseStatus, bool expected)
        {
            // Arrange
            var account = _fixture.Create<Models.Account>();
            account.VrfCaseStatus = vrfCaseStatus;

            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();

            // Act
            var actual = (await _sut.GetList(x => x.AccountId == account.Id)).Single();

            // Assert
            actual.BankDetailsRequired.Should().Be(expected);
        }
    }
}