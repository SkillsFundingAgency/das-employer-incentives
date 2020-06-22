using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.UnitTests.TestHelpers;
using SFA.DAS.EmployerIncentives.Domain.Data;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static SFA.DAS.EmployerIncentives.Data.UnitTests.TestHelpers.SqlHelper;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.AccountDataRepositoryTests
{
    public class WhenAddCalled
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
        public async Task Then_the_account_is_added_to_the_data_store()
        {
            // Arrange
            var testLegalEntity = _fixture.Create<LegalEntityModel>();
            var testAccount = _fixture
                .Build<AccountModel>()
                .With(f => f.LegalEntityModels, new List<ILegalEntityModel> { testLegalEntity })
                .Create();

            (await _sut.Find(testAccount.Id)).Should().BeNull();

            // Act
            await _sut.Add(testAccount);
            var account = await _sut.Find(testAccount.Id);

            // Assert
            account.Should().NotBeNull();
            account.Id.Should().Be(testAccount.Id);
            var legalEntity = account.LegalEntityModels.Single();
            legalEntity.Id.Should().Be(testLegalEntity.Id);
            legalEntity.Name.Should().Be(testLegalEntity.Name);
            legalEntity.AccountLegalEntityId.Should().Be(testLegalEntity.AccountLegalEntityId);
        }
    }
}
