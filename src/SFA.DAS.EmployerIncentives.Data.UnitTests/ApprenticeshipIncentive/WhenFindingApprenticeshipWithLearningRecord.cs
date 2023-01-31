using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.ApprenticeshipIncentive
{
    [TestFixture]
    [NonParallelizable]
    public class WhenFindingApprenticeshipWithLearningRecord
    {
        private ApprenticeshipIncentives.ApprenticeshipIncentiveDataRepository _sut;
        private Fixture _fixture;
        private EmployerIncentivesDbContext _dbContext;
        private long _accountLegalEntityId;
        private Mock<IServiceProvider> _mockServiceProvider;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _accountLegalEntityId = _fixture.Create<long>();
            _mockServiceProvider = new Mock<IServiceProvider>();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options, _mockServiceProvider.Object);

            _sut = new ApprenticeshipIncentives.ApprenticeshipIncentiveDataRepository(new Lazy<EmployerIncentivesDbContext>(_dbContext));
        }

        [Test]
        public async Task Then_the_apprenticeship_incentives_are_returned_if_learning_found()
        {
            // Arrange
            await AddApprenticeshipIncentive(_accountLegalEntityId, _fixture.Create<long>(), true, true);
            await AddApprenticeshipIncentive(_accountLegalEntityId, _fixture.Create<long>(), true, true);

            // Act
            var incentives = await _sut.FindIncentivesWithLearningFound();

            // Assert
            incentives.Should().NotBeNull();
            incentives.Count.Should().Be(2);
        }

        [Test]
        public async Task Then_the_apprenticeship_incentive_is_not_returned_if_no_learning_record_found()
        {
            // Arrange
            await AddApprenticeshipIncentive(_accountLegalEntityId, _fixture.Create<long>(), true, true);
            await AddApprenticeshipIncentive(_accountLegalEntityId, _fixture.Create<long>(), true, false);

            // Act
            var incentives = await _sut.FindIncentivesWithLearningFound();

            // Assert
            incentives.Should().NotBeNull();
            incentives.Count.Should().Be(1);
        }


        [Test]
        public async Task Then_the_apprenticeship_incentive_is_not_returned_if_no_learner_match_record_for_uln()
        {
            // Arrange
            await AddApprenticeshipIncentive(_accountLegalEntityId, _fixture.Create<long>(), false, false);
            await AddApprenticeshipIncentive(_accountLegalEntityId, _fixture.Create<long>(), true, true);

            // Act
            var incentives = await _sut.FindIncentivesWithLearningFound();

            // Assert
            incentives.Should().NotBeNull();
            incentives.Count.Should().Be(1);
        }
        
        [Test]
        public async Task Then_the_apprenticeship_incentive_is_not_returned_if_the_status_is_withdrawn()
        {
            // Arrange
            await AddApprenticeshipIncentive(_accountLegalEntityId, _fixture.Create<long>(), true, true);
            await AddApprenticeshipIncentive(_accountLegalEntityId, _fixture.Create<long>(), true, true, IncentiveStatus.Withdrawn);

            // Act
            var incentives = await _sut.FindIncentivesWithLearningFound();

            // Assert
            incentives.Should().NotBeNull();
            incentives.Count.Should().Be(1);
        }

        [TearDown]
        public void CleanUp()
        {
            _dbContext.Dispose();
        }

        private async Task AddApprenticeshipIncentive(long accountLegalEntityId, long uln, bool hasLearningRecord, 
                                                      bool learningFound, IncentiveStatus status = IncentiveStatus.Active)
        {
            var incentive = _fixture.Build<Data.ApprenticeshipIncentives.Models.ApprenticeshipIncentive>()
                .With(x => x.AccountLegalEntityId, accountLegalEntityId)
                .With(x => x.ULN, uln)
                .With(x => x.BreakInLearnings, new List<ApprenticeshipBreakInLearning>())
                .With(x => x.Status, status)
                .Create();
            
            await _dbContext.AddAsync(incentive);
            if (hasLearningRecord)
            {
                var learner = _fixture.Build<ApprenticeshipIncentives.Models.Learner>()
                    .With(x => x.LearningFound, learningFound)
                    .With(x => x.ApprenticeshipIncentiveId, incentive.Id)
                    .Create();
                await _dbContext.AddAsync(learner);
            }
            await _dbContext.SaveChangesAsync();
        }
    }
}
