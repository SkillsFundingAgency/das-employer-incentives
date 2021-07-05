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
using LearningPeriod = SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes.LearningPeriod;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.LearnerDataRepository
{
    public class WhenUpdateCalled
    {
        private ApprenticeshipIncentives.LearnerDataRepository _sut;
        private readonly Fixture _fixture = new Fixture();
        private EmployerIncentivesDbContext _dbContext;

        [SetUp]
        public void Arrange()
        {
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
        public async Task Then_an_existing_learner_is_updated_in_the_data_store()
        {
            // Arrange
            var existingLearner =
                _fixture.Build<ApprenticeshipIncentives.Models.Learner>()
                .Create();
            _dbContext.Learners.Add(existingLearner);
            await _dbContext.SaveChangesAsync();

            var submissionData = _fixture.Create<SubmissionData>();
            submissionData.SetSubmissionDate(_fixture.Create<DateTime>());
            submissionData.SetLearningData(new LearningData(true));
            submissionData.LearningData.SetHasDataLock(true);
            submissionData.SetRawJson(_fixture.Create<string>());
            submissionData.LearningData.SetStartDate(_fixture.Create<DateTime>());
            submissionData.LearningData.SetIsInLearning(true);
            submissionData.LearningData.SetIsStopped(new LearningStoppedStatus(false, _fixture.Create<DateTime>()));

            var testLearner =
                _fixture.Build<LearnerModel>()
                .With(l => l.Id, existingLearner.Id)
                .With(l => l.SubmissionData, submissionData)
                .Create();

            // Act
            await _sut.Update(testLearner);
            await _dbContext.SaveChangesAsync();

            // Assert
            _dbContext.Learners.Count().Should().Be(1);
            var storedLearner = _dbContext.Learners.Single();
            storedLearner.Id.Should().Be(testLearner.Id);
            storedLearner.ApprenticeshipIncentiveId.Should().Be(testLearner.ApprenticeshipIncentiveId);
            storedLearner.ApprenticeshipId.Should().Be(testLearner.ApprenticeshipId);
            storedLearner.Ukprn.Should().Be(testLearner.Ukprn);
            storedLearner.ULN.Should().Be(testLearner.UniqueLearnerNumber);
            storedLearner.SubmissionFound.Should().Be(true);
            storedLearner.SubmissionDate.Should().Be(testLearner.SubmissionData.SubmissionDate);
            storedLearner.LearningFound.Should().Be(testLearner.SubmissionData.LearningData.LearningFound);
            storedLearner.StartDate.Should().Be(testLearner.SubmissionData.LearningData.StartDate);
            storedLearner.HasDataLock.Should().BeTrue();
            storedLearner.InLearning.Should().BeTrue();
            storedLearner.RawJSON.Should().Be(testLearner.SubmissionData.RawJson);
            storedLearner.LearningResumedDate.Should().Be(testLearner.SubmissionData.LearningData.StoppedStatus.DateResumed);
            storedLearner.LearningStoppedDate.Should().BeNull();
            storedLearner.SuccessfulLearnerMatch.Should().Be(testLearner.SuccessfulLearnerMatch);
        }

        [Test]
        public async Task Then_existing_learning_periods_are_updated_in_the_data_store()
        {
            // Arrange
            var existingLearner = _fixture.Create<ApprenticeshipIncentives.Models.Learner>();
            await _dbContext.AddAsync(existingLearner);
            await _dbContext.SaveChangesAsync();

            var testLearner =
                _fixture.Build<LearnerModel>()
                .With(l => l.Id, existingLearner.Id)
                .Create();

            var lp = existingLearner.LearningPeriods.First();
            var expected = new LearningPeriod(lp.StartDate, existingLearner.LearningPeriods.First().StartDate.AddMonths(10));
            testLearner.LearningPeriods.Add(expected);

            // Act
            await _sut.Update(testLearner);
            await _dbContext.SaveChangesAsync();

            // Assert
            var storedLearner = _dbContext.Learners.Single();
            storedLearner.LearningPeriods.Should().HaveCount(4);
            storedLearner.LearningPeriods.Any(x => x.StartDate == expected.StartDate && x.EndDate == expected.EndDate).Should().BeTrue();
        }

        [Test]
        public async Task Then_existing_days_in_learning_are_updated_in_the_data_store()
        {
            // Arrange
            var existingLearner = _fixture.Create<ApprenticeshipIncentives.Models.Learner>();
            await _dbContext.AddAsync(existingLearner);
            await _dbContext.SaveChangesAsync();

            var testLearner =
                _fixture.Build<LearnerModel>()
                    .With(l => l.Id, existingLearner.Id)
                    .Create();

            var d = existingLearner.DaysInLearnings.First();
            var expected = new DaysInLearning(d.CollectionPeriodNumber, d.CollectionPeriodYear, _fixture.Create<int>());
            testLearner.DaysInLearnings.Add(expected);

            // Act
            await _sut.Update(testLearner);
            await _dbContext.SaveChangesAsync();

            // Assert
            var storedLearner = _dbContext.Learners.Single();
            storedLearner.DaysInLearnings.Should().HaveCount(4);
            storedLearner.DaysInLearnings.Any(x => x.CollectionPeriodNumber == expected.CollectionPeriodNumber
                                                   && x.CollectionPeriodYear == expected.CollectionYear
                                                   && x.NumberOfDaysInLearning == expected.NumberOfDays)
                .Should().BeTrue();
        }
    }
}
