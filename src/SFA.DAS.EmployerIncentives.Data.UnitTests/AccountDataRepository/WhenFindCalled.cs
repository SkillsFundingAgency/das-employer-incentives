using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Map;
using SFA.DAS.EmployerIncentives.Data.Tables;
using SFA.DAS.EmployerIncentives.Data.UnitTests.TestHelpers;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static SFA.DAS.EmployerIncentives.Data.UnitTests.TestHelpers.SqlHelper;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.AccountDataRepositoryTests
{
    public class WhenFindCalled
    {
        private AccountDataRepository _sut;
        private Fixture _fixture;
        private Mock<IOptions<FunctionSettings>> _mockOptions;
        private DatabaseProperties _dbProperties;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            
            _dbProperties = SqlHelper.CreateTestDatabase();

            _mockOptions = new Mock<IOptions<FunctionSettings>>();
            _mockOptions
                .Setup(m => m.Value)
                .Returns(new FunctionSettings { DbConnectionString = $"{_dbProperties.ConnectionString}" });

            _sut = new AccountDataRepository(_mockOptions.Object);
        }

        [TearDown]
        public void CleanUp()
        {
            SqlHelper.DeleteTestDatabase(_dbProperties);
        }

        [Test]
        public async Task Then_the_expected_account_is_returned_if_it_exists()
        {
            // Arrange
            var testAccount = _fixture.Create<Account>();
            var accounts = new List<Account> { testAccount };
            await _sut.Add((new List<Account> { _fixture.Create<Account>() }).MapSingle());
            await _sut.Add(accounts.MapSingle());
            await _sut.Add((new List<Account> { _fixture.Create<Account>() }).MapSingle());

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
