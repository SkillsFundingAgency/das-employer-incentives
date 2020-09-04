using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.ApprenticeshipIncentive
{
    public class WhenUpdateCalled
    {
        private ApprenticeshipIncentives.ApprenticeshipIncentiveDataRepository _sut;
        private Fixture _fixture;
        private EmployerIncentivesDbContext _dbContext;
        private ApprenticeshipIncentiveModel _testIncentive;

        [SetUp]
        public async Task Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options);

            _testIncentive = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .Create();

            _sut = new ApprenticeshipIncentives.ApprenticeshipIncentiveDataRepository(_dbContext);
            await _sut.Add(_testIncentive);
        }

        [TearDown]
        public void CleanUp()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task Then_the_apprenticeship_incentive_is_updated_with_new_values()
        {
            // Act
            var storedIncentive = await _sut.Get(_testIncentive.Id);

            storedIncentive.Account = _fixture.Create<Domain.ApprenticeshipIncentives.ValueTypes.Account>();
            storedIncentive.Apprenticeship = _fixture.Create<Domain.ApprenticeshipIncentives.ValueTypes.Apprenticeship>();
            
            await _sut.Update(storedIncentive);

            // Assert
            _dbContext.ApprenticeshipIncentives.Count().Should().Be(1);
            _dbContext.ApprenticeshipIncentives.Count(
                x => x.AccountId == storedIncentive.Account.Id &&
                     x.ApprenticeshipId == storedIncentive.Apprenticeship.Id &&
                     x.FirstName == storedIncentive.Apprenticeship.FirstName &&
                     x.LastName == storedIncentive.Apprenticeship.LastName &&
                     x.DateOfBirth == storedIncentive.Apprenticeship.DateOfBirth &&
                     x.Uln == storedIncentive.Apprenticeship.UniqueLearnerNumber &&
                     x.EmployerType == storedIncentive.Apprenticeship.EmployerType
                )
                .Should().Be(1);
        }
    }
}
