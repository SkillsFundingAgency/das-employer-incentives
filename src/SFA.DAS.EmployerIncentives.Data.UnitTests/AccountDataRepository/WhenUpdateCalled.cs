using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.AccountDataRepository
{
    public class WhenUpdateCalled
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
        public async Task Then_the_account_is_added_if_it_does_not_exist()
        {
            // Arrange
            var testLegalEntity = _fixture.Create<LegalEntityModel>();
            var testAccount = _fixture
                .Build<AccountModel>()
                .With(f => f.LegalEntityModels, new List<LegalEntityModel> { testLegalEntity })
                .Create();

            // Act
            await _sut.Update(testAccount);
            await _dbContext.SaveChangesAsync();

            // Assert
            _dbContext.Accounts.Count().Should().Be(1);

            var storedAccount = _dbContext.Accounts.Single();
            storedAccount.Id.Should().Be(testAccount.Id);
            storedAccount.LegalEntityId.Should().Be(testLegalEntity.Id);
            storedAccount.AccountLegalEntityId.Should().Be(testLegalEntity.AccountLegalEntityId);
            storedAccount.LegalEntityName.Should().Be(testLegalEntity.Name);
            storedAccount.SignedAgreementVersion.Should().Be(testLegalEntity.SignedAgreementVersion);
            storedAccount.HashedLegalEntityId.Should().Be(testLegalEntity.HashedLegalEntityId);
        }

        [Test]
        public async Task Then_the_account_is_updated_if_it_already_exists()
        {
            // Arrange
            var testAccount = _fixture.Create<Models.Account>();
            _dbContext.Add(_fixture.Create<Models.Account>());
            _dbContext.Add(testAccount);
            _dbContext.Add(_fixture.Create<Models.Account>());
            _dbContext.SaveChanges();
            var newName = testAccount.LegalEntityName + "changed";
            var newSignedVersion = testAccount.SignedAgreementVersion + 1;

            var legalEntities = new List<LegalEntityModel>
            {
                new LegalEntityModel
                {
                    Id = testAccount.LegalEntityId,
                    AccountLegalEntityId = testAccount.AccountLegalEntityId,
                    Name = newName,
                    SignedAgreementVersion = newSignedVersion
                }
            };

            var accountModel = new AccountModel { Id = testAccount.Id, LegalEntityModels = legalEntities };

            // Act
            await _sut.Update(accountModel);
            await _dbContext.SaveChangesAsync();

            // Assert
            var addedAccount = _dbContext.Accounts.Single(a => a.Id == testAccount.Id && a.AccountLegalEntityId == testAccount.AccountLegalEntityId);
            addedAccount.LegalEntityId.Should().Be(testAccount.LegalEntityId);
            addedAccount.LegalEntityName.Should().Be(newName);
            addedAccount.SignedAgreementVersion.Should().Be(newSignedVersion);
            addedAccount.VrfCaseId.Should().Be(testAccount.VrfCaseId);
            addedAccount.VrfCaseStatus.Should().Be(testAccount.VrfCaseStatus);
            addedAccount.VrfVendorId.Should().Be(testAccount.VrfVendorId);
            addedAccount.VrfCaseStatusLastUpdatedDateTime.Should().Be(testAccount.VrfCaseStatusLastUpdatedDateTime);
            addedAccount.HashedLegalEntityId.Should().Be(testAccount.HashedLegalEntityId);
        }

        [Test]
        public async Task Then_the_matching_account_row_is_deleted_if_it_already_exists()
        {
            // Arrange
            var testAccount = _fixture.Create<Models.Account>();
            var testAccount2 = _fixture.Build<Models.Account>().With(a => a.Id, testAccount.Id).Create();

            _dbContext.Add(testAccount);
            _dbContext.Add(testAccount2);
            _dbContext.SaveChanges();

            var legalEntities = new List<LegalEntityModel>
            {
                new LegalEntityModel
                {
                    Id = testAccount.LegalEntityId,
                    AccountLegalEntityId = testAccount.AccountLegalEntityId,
                    Name = testAccount.LegalEntityName
                }
            };

            var accountModel = new AccountModel { Id = testAccount.Id, LegalEntityModels = legalEntities };

            // Act
            await _sut.Update(accountModel);
            await _dbContext.SaveChangesAsync();

            // Assert
            _dbContext.Accounts.Count().Should().Be(1);

            var storedAccount = _dbContext.Accounts.Single(a => a.Id == testAccount.Id && a.AccountLegalEntityId == testAccount.AccountLegalEntityId);
            storedAccount.LegalEntityId.Should().Be(testAccount.LegalEntityId);
            storedAccount.LegalEntityName.Should().Be(testAccount.LegalEntityName);
            storedAccount.SignedAgreementVersion.Should().Be(testAccount.SignedAgreementVersion);
        }

        [Test]
        public async Task Then_the_account_is_deleted_if_all_legal_entities_are_deleted()
        {
            // Arrange
            var testAccount = _fixture.Create<Models.Account>();

            _dbContext.Add(testAccount);
            _dbContext.SaveChanges();

            var legalEntities = new List<LegalEntityModel>();

            var accountModel = new AccountModel { Id = testAccount.Id, LegalEntityModels = legalEntities };

            // Act
            await _sut.Update(accountModel);
            await _dbContext.SaveChangesAsync();

            // Assert
            _dbContext.Accounts.Count().Should().Be(0);
        }

    }
}
