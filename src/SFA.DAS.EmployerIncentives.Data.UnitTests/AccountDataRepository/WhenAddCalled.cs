using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.AccountDataRepository
{
    public class WhenAddCalled
    {
        private Data.AccountDataRepository _sut;
        private Fixture _fixture;
        private EmployerIncentivesDbContext _dbContext;
        private Mock<IServiceProvider> _mockServiceProvider;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _mockServiceProvider = new Mock<IServiceProvider>();
            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options, _mockServiceProvider.Object);

            _sut = new Data.AccountDataRepository(new Lazy<EmployerIncentivesDbContext>(_dbContext));
        }

        [TearDown]
        public void CleanUp()
        {
            _dbContext.Dispose();
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
            await _dbContext.SaveChangesAsync();

            // Assert
            _dbContext.Accounts.Count().Should().Be(1);

            var storedAccount = _dbContext.Accounts.Single();
            storedAccount.Id.Should().Be(testAccount.Id);
            storedAccount.HashedLegalEntityId.Should().Be(testLegalEntity.HashedLegalEntityId);
            storedAccount.LegalEntityId.Should().Be(testLegalEntity.Id);
            storedAccount.AccountLegalEntityId.Should().Be(testLegalEntity.AccountLegalEntityId);
            storedAccount.LegalEntityName.Should().Be(testLegalEntity.Name);
            storedAccount.VrfCaseId.Should().Be(testLegalEntity.VrfCaseId);
            storedAccount.VrfCaseStatus.Should().Be(testLegalEntity.VrfCaseStatus);
            storedAccount.VrfVendorId.Should().Be(testLegalEntity.VrfVendorId);
            storedAccount.VrfCaseStatusLastUpdatedDateTime.Should().Be(testLegalEntity.VrfCaseStatusLastUpdatedDateTime);
        }
    }
}
