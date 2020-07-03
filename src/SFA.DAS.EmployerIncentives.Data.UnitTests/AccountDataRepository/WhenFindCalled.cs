using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Dapper;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Tables;
using SFA.DAS.EmployerIncentives.Data.UnitTests.TestHelpers;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.AccountDataRepository
{
    public class WhenFindCalled
    {
        private Data.AccountDataRepository _sut;
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

            _sut = new Data.AccountDataRepository(_mockOptions.Object);
        }

        [TearDown]
        public void CleanUp()
        {
            _sqlDb.Dispose();
        }

        [Test]
        public async Task Then_the_expected_account_is_returned_if_it_exists()
        {
            // Arrange
            var testAccount = _fixture.Create<AccountTable>();
            using (var dbConnection = new SqlConnection(_sqlDb.DatabaseInfo.ConnectionString))
            {
                await dbConnection.InsertAsync(testAccount);
            }

            // Act
            var account = await _sut.Find(testAccount.Id);

            // Assert
            account.Should().NotBeNull();
            account.Id.Should().Be(testAccount.Id);
            var legalEntity = account.LegalEntityModels.Single();
            legalEntity.Id.Should().Be(testAccount.LegalEntityId);
            legalEntity.Name.Should().Be(testAccount.LegalEntityName);
            legalEntity.AccountLegalEntityId.Should().Be(testAccount.AccountLegalEntityId);
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
    }
}
