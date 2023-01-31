using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.LearnerDataRepository
{
    public class WhenGetByApprenticeshipIncentiveIdCalled
    {
        private ApprenticeshipIncentives.LearnerDataRepository _sut;
        private readonly Fixture _fixture = new Fixture();
        private EmployerIncentivesDbContext _dbContext;
        private Mock<IServiceProvider> _mockServiceProvider;

        [SetUp]
        public void Arrange()
        {
            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>().UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _mockServiceProvider = new Mock<IServiceProvider>();

            _dbContext = new EmployerIncentivesDbContext(options, _mockServiceProvider.Object);
            _sut = new ApprenticeshipIncentives.LearnerDataRepository(new Lazy<EmployerIncentivesDbContext>(_dbContext));
        }

        [TearDown]
        public void CleanUp() => _dbContext.Dispose();

        [Test]
        public async Task Then_the_learner_is_retrieved()
        {
            var submissionData = _fixture.Create<SubmissionData>();
            submissionData.SetLearningData(new LearningData(true));
            submissionData.SetSubmissionDate(_fixture.Create<DateTime>());
            submissionData.LearningData.SetHasDataLock(true);
            submissionData.SetRawJson(_fixture.Create<string>());

            var periods = new[]
            {
            new LearningPeriod(new DateTime(2020, 9, 1),new DateTime(2021, 6, 30)),
            new LearningPeriod(new DateTime(2021, 8, 1),new DateTime(2022, 7, 31)),
            };

            var testLearner =
            _fixture.Build<LearnerModel>()
            .With(l => l.LearningPeriods, periods)
            .With(l => l.SubmissionData, submissionData)
            .Create();

            // Act
            await _sut.Add(testLearner);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _sut.GetByApprenticeshipIncentiveId(testLearner.ApprenticeshipIncentiveId);

            // Assert            
            result.Id.Should().Be(testLearner.Id);
            result.ApprenticeshipIncentiveId.Should().Be(testLearner.ApprenticeshipIncentiveId);
            result.ApprenticeshipId.Should().Be(testLearner.ApprenticeshipId);
            result.Ukprn.Should().Be(testLearner.Ukprn);
            result.UniqueLearnerNumber.Should().Be(testLearner.UniqueLearnerNumber);
            result.SubmissionData.Should().NotBeNull();
            result.SubmissionData.SubmissionDate.Should().Be(testLearner.SubmissionData.SubmissionDate);
            result.SubmissionData.LearningData.IsInlearning.Should().Be(testLearner.SubmissionData.LearningData.IsInlearning);
            result.SubmissionData.LearningData.HasDataLock.Should().BeTrue();
            result.SubmissionData.LearningData.StartDate.Should().BeNull();
            result.SubmissionData.LearningData.IsInlearning.Should().BeNull();
            result.SubmissionData.RawJson.Should().Be(testLearner.SubmissionData.RawJson);
        }

        [Test(Description = "This test is required to validate data persisted in a format prior to changes made for EI-632/636 can be reloaded")]
        public async Task Then_the_learner_is_retrieved_when_learning_found_is_null()
        {
            var testLearner = _fixture.Build<ApprenticeshipIncentives.Models.Learner>().Without(l=>l.LearningPeriods).Create();
            testLearner.SubmissionFound = false;
            testLearner.SubmissionDate = null;
            testLearner.LearningFound = null;

            // Act
            await _dbContext.AddAsync(testLearner);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _sut.GetByApprenticeshipIncentiveId(testLearner.ApprenticeshipIncentiveId);

            // Assert            
            result.Should().NotBeNull();
        }


        [Test]
        public async Task Then_null_is_returned_when_no_data_found()
        {
            // Act
            var result = await _sut.GetByApprenticeshipIncentiveId(Guid.Empty);

            // Assert
            result.Should().BeNull();
        }
    }
}
