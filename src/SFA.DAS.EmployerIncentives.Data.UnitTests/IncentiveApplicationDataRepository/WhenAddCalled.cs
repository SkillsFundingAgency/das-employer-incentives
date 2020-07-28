using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplication.Models;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.IncentiveApplicationDataRepository
{
    public class WhenAddCalled
    {
        private Data.IncentiveApplicationDataRepository _sut;
        private Fixture _fixture;
        private EmployerIncentivesDbContext _dbContext;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options);

            _sut = new Data.IncentiveApplicationDataRepository(_dbContext);
        }

        [TearDown]
        public void CleanUp()
        {            
            _dbContext.Dispose();
         }

        [Test]
        public async Task Then_the_incentive_application_is_added_to_the_data_store()
        {
            // Arrange
            var testApplication = _fixture
                .Build<IncentiveApplicationModel>()
                .Create();

            // Act
            await _sut.Add(testApplication);

            // Assert
            _dbContext.Applications.Count().Should().Be(1);

            var storedAccount = _dbContext.Applications.Single();
            storedAccount.Id.Should().Be(testApplication.Id);
            storedAccount.AccountId.Should().Be(testApplication.AccountId);
            storedAccount.AccountLegalEntityId.Should().Be(testApplication.AccountLegalEntityId);
            storedAccount.DateCreated.Should().Be(testApplication.DateCreated);
            storedAccount.DateSubmitted.Should().Be(testApplication.DateSubmitted);
            storedAccount.Status.Should().Be(testApplication.Status);
            storedAccount.SubmittedBy.Should().Be(testApplication.SubmittedBy);
        }
    }
}
