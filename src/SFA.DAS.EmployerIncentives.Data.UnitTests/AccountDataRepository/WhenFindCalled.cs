using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Map;
using SFA.DAS.EmployerIncentives.Data.Tables;
using SFA.DAS.EmployerIncentives.Data.UnitTests.TestHelpers;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
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
            await _sut.Add(testAccount.Map());

            // Act
            var account = await _sut.Find(testAccount.Id);

            // Assert
            account.Should().NotBeNull();
            account.Id.Should().Be(testAccount.Id);
            account.AccountLegalEntityId.Should().Be(testAccount.AccountLegalEntityId);
            account.LegalEntityModel.Id.Should().Be(testAccount.LegalEntityId);
            account.LegalEntityModel.Name.Should().Be(testAccount.LegalEntityName);
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
