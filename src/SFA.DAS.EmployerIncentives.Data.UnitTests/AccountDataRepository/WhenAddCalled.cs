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
    public class WhenAddCalled
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

            _sut = new Data.AccountDataRepository(_dbContext);
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

            // Assert
            _dbContext.Accounts.Count().Should().Be(1);

            var storedAccount = _dbContext.Accounts.Single();
            storedAccount.Id.Should().Be(testAccount.Id);
            storedAccount.LegalEntityId.Should().Be(testLegalEntity.Id);
            storedAccount.AccountLegalEntityId.Should().Be(testLegalEntity.AccountLegalEntityId);
            storedAccount.LegalEntityName.Should().Be(testLegalEntity.Name);            
        }
    }
}
