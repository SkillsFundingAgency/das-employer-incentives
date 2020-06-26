using AutoFixture;
using Dapper;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SFA.DAS.EmployerIncentives.Data.Map;
using SFA.DAS.EmployerIncentives.Data.Tables;
using SFA.DAS.EmployerIncentives.Data.UnitTests.TestHelpers;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.AccountDataRepositoryTests
{
    public class WhenUpdateCalled
    {
        private AccountDataRepository _sut;
        private Fixture _fixture;
        private Mock<IOptions<ApplicationSettings>> _mockOptions;
        private SqlDatabase _sqlDb;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();            
            _sqlDb = new SqlDatabase();

            _mockOptions = new Mock<IOptions<ApplicationSettings>>();
            _mockOptions
                .Setup(m => m.Value)
                .Returns(new ApplicationSettings { DbConnectionString = $"{_sqlDb.DatabaseInfo.ConnectionString}" });

            using (var dbConnection = new SqlConnection(_sqlDb.DatabaseInfo.ConnectionString))
            {
                dbConnection.ExecuteAsync("TRUNCATE TABLE Accounts");
            }

            _sut = new AccountDataRepository(_mockOptions.Object);
        }

        [TearDown]
        public void CleanUp()
        {
            _sqlDb.Dispose();
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

            // Assert
            using (var dbConnection = new SqlConnection(_sqlDb.DatabaseInfo.ConnectionString))
            {
                var accounts = await dbConnection.QueryAsync<Account>("SELECT * FROM Accounts");

                var storedAccount = accounts.Single();
                storedAccount.Id.Should().Be(testAccount.Id);
                storedAccount.LegalEntityId.Should().Be(testLegalEntity.Id);
                storedAccount.AccountLegalEntityId.Should().Be(testLegalEntity.AccountLegalEntityId);
                storedAccount.LegalEntityName.Should().Be(testLegalEntity.Name);
            }
        }

        [Test]
        public async Task Then_the_account_is_updated_if_it_already_exists()
        {
            // Arrange
            var testAccount = _fixture.Create<Account>();
            using (var dbConnection = new SqlConnection(_sqlDb.DatabaseInfo.ConnectionString))
            {
                await dbConnection.InsertAsync(testAccount);
            }
            
            var legalEntities = new List<LegalEntityModel>
            {
                new LegalEntityModel
                {
                    Id = testAccount.LegalEntityId,
                    AccountLegalEntityId = testAccount.AccountLegalEntityId,
                    Name = testAccount.LegalEntityName + "changed"
                }
            };

            var accountModel = new AccountModel { Id = testAccount.Id, LegalEntityModels = legalEntities };

            // Act
            await _sut.Update(accountModel);

            // Assert
            using (var dbConnection = new SqlConnection(_sqlDb.DatabaseInfo.ConnectionString))
            {
                var accounts = await dbConnection.QueryAsync<Account>("SELECT * FROM Accounts");

                var addedAccount = accounts.Single(l => l.Id == testAccount.Id && l.AccountLegalEntityId == testAccount.AccountLegalEntityId);
                addedAccount.LegalEntityId.Should().Be(testAccount.LegalEntityId);
                addedAccount.LegalEntityName.Should().Be(testAccount.LegalEntityName + "changed");
            }
        }

        [Test]
        public async Task Then_the_matching_account_row_is_deleted_if_it_already_exists()
        {
            // Arrange
            var testAccount = _fixture.Create<Account>();
            var testAccount2 = _fixture.Build<Account>().With(a => a.Id, testAccount.Id).Create();
            using (var dbConnection = new SqlConnection(_sqlDb.DatabaseInfo.ConnectionString))
            {
                await dbConnection.InsertAsync(testAccount);
                await dbConnection.InsertAsync(testAccount2);
            }

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

            // Assert
            using (var dbConnection = new SqlConnection(_sqlDb.DatabaseInfo.ConnectionString))
            {
                var accounts = await dbConnection.QueryAsync<Account>("SELECT * FROM Accounts");

                accounts.Count().Should().Be(1);
                var addedAccount = accounts.Single(l => l.Id == testAccount.Id && l.AccountLegalEntityId == testAccount.AccountLegalEntityId);
                addedAccount.LegalEntityId.Should().Be(testAccount.LegalEntityId);
                addedAccount.LegalEntityName.Should().Be(testAccount.LegalEntityName);
            }
        }

        [Test]
        public async Task Then_the_account_is_deleted_if_all_legal_entities_are_deleted()
        {
            // Arrange
            var testAccount = _fixture.Create<Account>();
            using (var dbConnection = new SqlConnection(_sqlDb.DatabaseInfo.ConnectionString))
            {
                await dbConnection.InsertAsync(testAccount);
            }

            var legalEntities = new List<LegalEntityModel>();

            var accountModel = new AccountModel { Id = testAccount.Id, LegalEntityModels = legalEntities };

            // Act
            await _sut.Update(accountModel);

            // Assert
            using (var dbConnection = new SqlConnection(_sqlDb.DatabaseInfo.ConnectionString))
            {
                var accounts = await dbConnection.QueryAsync<Account>("SELECT * FROM Accounts");

                accounts.Count().Should().Be(0);
            }
        }

    }
}
