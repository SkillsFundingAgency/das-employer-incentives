﻿using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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
        public async Task Then_the_learner_is_retrieved()
        {
            var submissionData = _fixture.Create<SubmissionData>();
            submissionData.SetLearningFound(new LearningFoundStatus());
            submissionData.SetHasDataLock(true);
            submissionData.SetRawJson(_fixture.Create<string>());

            var testLearner =
                _fixture.Build<LearnerModel>()
                .With(l => l.SubmissionData, submissionData)
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
            result.SubmissionData.LearningFoundStatus.Should().Be(new LearningFoundStatus(testLearner.SubmissionData.LearningFoundStatus.LearningFound));
            result.SubmissionData.HasDataLock.Should().BeTrue();
            result.SubmissionData.StartDate.Should().BeNull();
            result.SubmissionData.IsInlearning.Should().BeNull();
            result.SubmissionData.RawJson.Should().Be(testLearner.SubmissionData.RawJson);
        }
    }
}