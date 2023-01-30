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
    public class WhenGetCalled
    {
        private ApprenticeshipIncentives.LearnerDataRepository _sut;
        private readonly Fixture _fixture = new Fixture();
        private EmployerIncentivesDbContext _dbContext;
        private Mock<IServiceProvider> _mockServiceProvider;

        [SetUp]
        public void Arrange()
        {
            _fixture.Customize<LearnerModel>(c => c.Without(x => x.LearningPeriods));
            _mockServiceProvider = new Mock<IServiceProvider>();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>().UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options, _mockServiceProvider.Object);
            _sut = new ApprenticeshipIncentives.LearnerDataRepository(new Lazy<EmployerIncentivesDbContext>(_dbContext));
        }

        [TearDown]
        public void CleanUp() => _dbContext.Dispose();

        [Test]
        public async Task Then_the_learner_is_returned_if_exists()
        {
            var submissionData = _fixture.Create<SubmissionData>();
            submissionData.SetSubmissionDate(_fixture.Create<DateTime>());
            submissionData.SetLearningData(new LearningData(true));
            submissionData.LearningData.SetHasDataLock(true);
            submissionData.SetRawJson(_fixture.Create<string>());
            
            var testLearner =
                _fixture.Build<LearnerModel>()
                .With(l => l.SubmissionData, submissionData)
                .Without(l => l.LearningPeriods)
                .Create();

            // Act
            await _sut.Add(testLearner);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _sut.Get(testLearner.Id);

            // Assert            
            result.Id.Should().Be(testLearner.Id);
            result.ApprenticeshipIncentiveId.Should().Be(testLearner.ApprenticeshipIncentiveId);
            result.ApprenticeshipId.Should().Be(testLearner.ApprenticeshipId);
            result.Ukprn.Should().Be(testLearner.Ukprn);
            result.UniqueLearnerNumber.Should().Be(testLearner.UniqueLearnerNumber);
            result.SubmissionData.Should().NotBeNull();
            result.SubmissionData.SubmissionDate.Should().Be(testLearner.SubmissionData.SubmissionDate);
            result.SubmissionData.LearningData.LearningFound.Should().Be(testLearner.SubmissionData.LearningData.LearningFound);
            result.SubmissionData.LearningData.HasDataLock.Should().BeTrue();
            result.SubmissionData.LearningData.StartDate.Should().BeNull();
            result.SubmissionData.LearningData.IsInlearning.Should().BeNull();
            result.SubmissionData.LearningData.StoppedStatus.LearningStopped.Should().BeFalse();
            result.SubmissionData.RawJson.Should().Be(testLearner.SubmissionData.RawJson);
        }

        [Test]
        public async Task Then_null_is_returned_if_does_not_exist()
        {
            // Act
            var result = await _sut.Get(Guid.Empty);

            // Assert            
            result.Should().BeNull();
        }
    }
}
