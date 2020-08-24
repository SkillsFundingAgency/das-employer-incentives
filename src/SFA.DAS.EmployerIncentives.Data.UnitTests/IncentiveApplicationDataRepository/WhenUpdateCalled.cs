using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.IncentiveApplicationDataRepository
{
    public class WhenUpdateCalled
    {
        private IncentiveApplication.IncentiveApplicationDataRepository _sut;
        private Fixture _fixture;
        private EmployerIncentivesDbContext _dbContext;
        private IncentiveApplicationModel _testApplication;

        [SetUp]
        public async Task Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options);

            _testApplication = _fixture
                .Build<IncentiveApplicationModel>()
                .Create();

            _sut = new IncentiveApplication.IncentiveApplicationDataRepository(_dbContext);
            await _sut.Add(_testApplication);
        }

        [TearDown]
        public void CleanUp()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task Then_the_incentive_application_is_updated_with_new_values()
        {
            // Act
            var storedApplication = await _sut.Get(_testApplication.Id);

            storedApplication.DateSubmitted = _fixture.Create<DateTime>();
            storedApplication.Status = Enums.IncentiveApplicationStatus.Submitted;
            storedApplication.SubmittedByEmail = _fixture.Create<string>();

            await _sut.Update(storedApplication);

            // Assert
            _dbContext.Applications.Count().Should().Be(1);
            _dbContext.Applications.Count(x => x.DateSubmitted == storedApplication.DateSubmitted)
                .Should().Be(1);
        }

        [Test]
        public async Task Then_the_incentive_application_apprenticeships_are_updated()
        {
            // Act
            var storedApplication = await _sut.Get(_testApplication.Id);
            storedApplication.ApprenticeshipModels.Remove(storedApplication.ApprenticeshipModels.First());
            var newApprenticeship = _fixture.Create<ApprenticeshipModel>();
            storedApplication.ApprenticeshipModels.Add(newApprenticeship);

            await _sut.Update(storedApplication);

            // Assert
            _dbContext.Applications.Single(x => x.Id == _testApplication.Id)
                .Apprenticeships.Should().BeEquivalentTo(storedApplication.ApprenticeshipModels);
        }
    }
}
