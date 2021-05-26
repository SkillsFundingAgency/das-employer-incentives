using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.IncentiveApplicationDataRepository
{
    public class WhenAddCalled
    {
        private IncentiveApplication.IncentiveApplicationDataRepository _sut;
        private Fixture _fixture;
        private EmployerIncentivesDbContext _dbContext;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options);

            _sut = new IncentiveApplication.IncentiveApplicationDataRepository(new Lazy<EmployerIncentivesDbContext>(_dbContext));
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
            await _dbContext.SaveChangesAsync();
            
            // Assert
            _dbContext.Applications.Count().Should().Be(1);

            var storedAccount = _dbContext.Applications.Single();
            storedAccount.Id.Should().Be(testApplication.Id);
            storedAccount.AccountId.Should().Be(testApplication.AccountId);
            storedAccount.AccountLegalEntityId.Should().Be(testApplication.AccountLegalEntityId);
            storedAccount.DateCreated.Should().Be(testApplication.DateCreated);
            storedAccount.DateSubmitted.Should().Be(testApplication.DateSubmitted);
            storedAccount.Status.Should().Be(testApplication.Status);
            storedAccount.SubmittedByEmail.Should().Be(testApplication.SubmittedByEmail);
            storedAccount.SubmittedByName.Should().Be(testApplication.SubmittedByName);            
        }

        [Test]
        public async Task Then_the_application_apprenticeships_are_added_to_the_data_store()
        {
            var testApprenticeship = _fixture.Create<ApprenticeshipModel>();

            // Arrange
            var testApplication = _fixture
                .Build<IncentiveApplicationModel>()
                .With(f => f.ApprenticeshipModels, new List<ApprenticeshipModel> { testApprenticeship })
                .Create();

            // Act
            await _sut.Add(testApplication);
            await _dbContext.SaveChangesAsync();

            // Assert
            _dbContext.ApplicationApprenticeships.Count().Should().Be(1);

            var storedApplication = _dbContext.Applications.Single();
            storedApplication.Apprenticeships.Count().Should().Be(1);
            var storedApprenticeship = storedApplication.Apprenticeships.Single();
            storedApprenticeship.Id.Should().Be(testApprenticeship.Id);
            storedApprenticeship.FirstName.Should().Be(testApprenticeship.FirstName);
            storedApprenticeship.LastName.Should().Be(testApprenticeship.LastName);
            storedApprenticeship.ApprenticeshipEmployerTypeOnApproval.Should().Be(testApprenticeship.ApprenticeshipEmployerTypeOnApproval);
            storedApprenticeship.ApprenticeshipId.Should().Be(testApprenticeship.ApprenticeshipId);
            storedApprenticeship.DateOfBirth.Should().Be(testApprenticeship.DateOfBirth);
            storedApprenticeship.IncentiveApplicationId.Should().Be(storedApplication.Id);
            storedApprenticeship.PlannedStartDate.Should().Be(testApprenticeship.PlannedStartDate);
            storedApprenticeship.ULN.Should().Be(testApprenticeship.ULN);
            storedApprenticeship.TotalIncentiveAmount.Should().Be(testApprenticeship.TotalIncentiveAmount);
            storedApprenticeship.UKPRN.Should().Be(testApprenticeship.UKPRN);
            storedApprenticeship.CourseName.Should().Be(testApprenticeship.CourseName);
            storedApprenticeship.Phase.Should().Be(testApprenticeship.Phase);
        }
    }
}
