using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
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
        private Mock<IServiceProvider> _mockServiceProvider;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _mockServiceProvider = new Mock<IServiceProvider>();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options, _mockServiceProvider.Object);

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
            submissionData.SetSubmissionDate(_fixture.Create<DateTime>());
            submissionData.SetLearningData(new LearningData(true));
            submissionData.LearningData.SetHasDataLock(true);
            submissionData.SetRawJson(_fixture.Create<string>());
            submissionData.LearningData.SetStartDate(_fixture.Create<DateTime>());
            submissionData.LearningData.SetIsInLearning(true);
            submissionData.LearningData.SetIsStopped(new LearningStoppedStatus(true, _fixture.Create<DateTime>()));
            
            var testLearner = 
                _fixture.Build<LearnerModel>()
                .With(l => l.SubmissionData, submissionData)
                .Without(l => l.LearningPeriods)
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
            storedLearner.LearningFound.Should().Be(testLearner.SubmissionData.LearningData.LearningFound);
            storedLearner.HasDataLock.Should().BeTrue();
            storedLearner.StartDate.Should().Be(testLearner.SubmissionData.LearningData.StartDate);
            storedLearner.InLearning.Should().BeTrue();
            storedLearner.RawJSON.Should().Be(testLearner.SubmissionData.RawJson);
            storedLearner.LearningStoppedDate.Should().Be(testLearner.SubmissionData.LearningData.StoppedStatus.DateStopped);
            storedLearner.LearningResumedDate.Should().BeNull();
        }
    }
}
