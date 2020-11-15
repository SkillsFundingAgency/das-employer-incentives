using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.Learner
{
    public class WhenAddCalled
    {
        private ApprenticeshipIncentives.LearnerDataRepository _sut;
        private Fixture _fixture;
        private EmployerIncentivesDbContext _dbContext;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options);

            _sut = new ApprenticeshipIncentives.LearnerDataRepository(new Lazy<EmployerIncentivesDbContext>(_dbContext));
        }

        [TearDown]
        public void CleanUp()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task Then_the_learner_is_added_to_the_data_store()
        {
            // Arrange
            var submissionData = _fixture.Create<SubmissionData>();
            submissionData.SetRawJson(_fixture.Create<string>());
            submissionData.SetStartDate(_fixture.Create<DateTime>());
            submissionData.SetIsInLearning(true);

            var testLearner = 
                _fixture.Build<LearnerModel>()
                .With(l => l.SubmissionData, submissionData)
                .Create();
            
            // Act
            await _sut.Add(testLearner);
            await _dbContext.SaveChangesAsync();

            // Assert
            _dbContext.Learners.Count().Should().Be(1);

            var storedLearner = _dbContext.Learners.Single();
            storedLearner.Id.Should().Be(testLearner.Id);
            storedLearner.ApprenticeshipIncentiveId.Should().Be(testLearner.ApprenticeshipIncentiveId);
            storedLearner.ApprenticeshipId.Should().Be(testLearner.ApprenticeshipId);
            storedLearner.Ukprn.Should().Be(testLearner.Ukprn);
            storedLearner.ULN.Should().Be(testLearner.UniqueLearnerNumber);
            storedLearner.SubmissionFound.Should().BeTrue();
            storedLearner.SubmissionDate.Should().Be(testLearner.SubmissionData.SubmissionDate);
            storedLearner.LearningFound.Should().Be(testLearner.SubmissionData.LearningFound);
            storedLearner.StartDate.Should().Be(testLearner.SubmissionData.StartDate);
            storedLearner.InLearning.Should().BeTrue();

            storedLearner.HasDataLock.Should().BeNull();            
            storedLearner.DaysInLearning.Should().BeNull();            
            storedLearner.RawJSON.Should().Be(testLearner.SubmissionData.RawJson);
            storedLearner.CreatedDate.Should().Be(testLearner.CreatedDate);
        }
    }
}
