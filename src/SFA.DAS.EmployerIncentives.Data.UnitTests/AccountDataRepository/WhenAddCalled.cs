using AutoFixture;
using Dapper;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
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
    public class WhenAddCalled
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
        public async Task Then_the_account_is_added_to_the_data_store()
        {
            // Arrange
            var testLegalEntity = _fixture.Create<LegalEntityModel>();
            var testAccount = _fixture
                .Build<AccountModel>()
                .With(f => f.LegalEntityModels, new List<LegalEntityModel> { testLegalEntity })
                .Create();

            // Act
            await _sut.Add(testAccount);

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
    }
}
