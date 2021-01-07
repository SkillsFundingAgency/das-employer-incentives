using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.EmployerIncentives.Enums;
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
            legalEntity.HashedLegalEntityId.Should().Be(testAccount.HashedLegalEntityId);
            legalEntity.VrfCaseStatusLastUpdatedDateTime.Should().Be(testAccount.VrfCaseStatusLastUpdatedDateTime);
            legalEntity.VrfCaseStatus.Should().Be(testAccount.VrfCaseStatus);
            legalEntity.VrfVendorId.Should().Be(testAccount.VrfVendorId);
            legalEntity.VrfCaseId.Should().Be(testAccount.VrfCaseId);
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

        [TestCase(LegalEntityVrfCaseStatus.Completed)]
        public async Task Then_the_correct_bank_details_status_is_set_for_completed_bank_details_journey(string vrfCaseStatus)
        {
            // Arrange
            var testAccount = _fixture.Create<Models.Account>();
            testAccount.VrfCaseStatus = vrfCaseStatus;
            _dbContext.Add(testAccount);
            _dbContext.SaveChanges();

            // Act
            var account = await _sut.Find(testAccount.Id);


            // Assert
            var legalEntity = account.LegalEntityModels.First();
            legalEntity.BankDetailsStatus.Should().Be(BankDetailsStatus.Completed);
        }

        [TestCase(LegalEntityVrfCaseStatus.RejectedDataValidation)]
        [TestCase(LegalEntityVrfCaseStatus.RejectedVer1)]
        [TestCase(LegalEntityVrfCaseStatus.RejectedVerification)]
        public async Task Then_the_correct_bank_details_status_is_set_for_rejected_bank_details(string vrfCaseStatus)
        {
            // Arrange
            var testAccount = _fixture.Create<Models.Account>();
            testAccount.VrfCaseStatus = vrfCaseStatus;
            _dbContext.Add(testAccount);
            _dbContext.SaveChanges();

            // Act
            var account = await _sut.Find(testAccount.Id);


            // Assert
            var legalEntity = account.LegalEntityModels.First();
            legalEntity.BankDetailsStatus.Should().Be(BankDetailsStatus.Rejected);
        }

        [TestCase(null)]
        [TestCase("")]
        public async Task Then_the_correct_bank_details_status_is_set_for_not_started_bank_details_journey(string vrfCaseStatus)
        {
            // Arrange
            var testAccount = _fixture.Create<Models.Account>();
            testAccount.VrfCaseStatus = vrfCaseStatus;
            _dbContext.Add(testAccount);
            _dbContext.SaveChanges();

            // Act
            var account = await _sut.Find(testAccount.Id);


            // Assert
            var legalEntity = account.LegalEntityModels.First();
            legalEntity.BankDetailsStatus.Should().Be(BankDetailsStatus.NotSupplied);
        }

        [TestCase(LegalEntityVrfCaseStatus.ToProcess)]
        public async Task Then_the_correct_bank_details_status_is_set_for_in_progress_bank_details_journey(string vrfCaseStatus)
        {
            // Arrange
            var testAccount = _fixture.Create<Models.Account>();
            testAccount.VrfCaseStatus = vrfCaseStatus;
            _dbContext.Add(testAccount);
            _dbContext.SaveChanges();

            // Act
            var account = await _sut.Find(testAccount.Id);


            // Assert
            var legalEntity = account.LegalEntityModels.First();
            legalEntity.BankDetailsStatus.Should().Be(BankDetailsStatus.InProgress);
        }
    }
}
