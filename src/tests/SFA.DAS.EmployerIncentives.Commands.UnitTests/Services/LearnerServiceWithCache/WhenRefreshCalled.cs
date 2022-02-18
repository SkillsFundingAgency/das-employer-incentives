using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.RefreshLearner;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.Services.LearnerServiceWithCache
{
    public class WhenRefreshCalled
    {
        private Mock<ILearnerService> _mockLearnerService;
        private Mock<ILearnerDomainRepository> _mockLearnerDomainRepository;
        private Mock<IDateTimeService> _mockDateTimeService;
        private ApplicationSettings _applicationSettings;
        private Domain.ApprenticeshipIncentives.ApprenticeshipIncentive _apprenticeshipIncentive;
        private Fixture _fixture;
        private Learner _refreshedLearner;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockLearnerService = new Mock<ILearnerService>();
            _mockLearnerDomainRepository = new Mock<ILearnerDomainRepository>();
            _mockDateTimeService = new Mock<IDateTimeService>();
            _applicationSettings = new ApplicationSettings();

            var model = _fixture.Create<ApprenticeshipIncentiveModel>();
            _apprenticeshipIncentive = Domain.ApprenticeshipIncentives.ApprenticeshipIncentive.Get(model.Id, _fixture.Create<ApprenticeshipIncentiveModel>());

            _refreshedLearner = Learner.Get(_fixture
                .Build<LearnerModel>()
                .Without(l => l.LearningPeriods)
                .Without(l => l.RefreshDate)
                .Create());

            _mockLearnerService
                .Setup(m => m.Refresh(_apprenticeshipIncentive))
                .ReturnsAsync(_refreshedLearner);
        }

        private Decorators.LearnerServiceWithCache Sut()
        {
            return new Decorators.LearnerServiceWithCache(
                _mockLearnerService.Object,
                _mockLearnerDomainRepository.Object,
                Options.Create(_applicationSettings),
                _mockDateTimeService.Object
                );
        }

        [Test]
        public async Task Then_the_learner_is_refreshed_when_no_config_exists()
        {
            //Arrange

            //Act
            var response = await Sut().Refresh(_apprenticeshipIncentive);

            //Assert
            _mockLearnerService.Verify(m => m.Refresh(_apprenticeshipIncentive), Times.Once());
            response.Should().Be(_refreshedLearner);
        }

        [Test]
        public async Task Then_the_learner_is_not_refreshed_when_config_exists_and_a_refresh_is_not_due()
        {
            //Arrange
            _applicationSettings = new ApplicationSettings
            {
                LearnerServiceCacheIntervalInMinutes = "60"
            };

            _mockDateTimeService
                .Setup(m => m.UtcNow())
                .Returns(DateTime.UtcNow);

            var learner = Learner.Get(_fixture
                .Build<LearnerModel>()
                .Without(l => l.LearningPeriods)
                .With(l => l.RefreshDate, DateTime.UtcNow.AddMinutes(-59))
                .Create());            

            _mockLearnerDomainRepository
               .Setup(m => m.Get(_apprenticeshipIncentive))
               .ReturnsAsync(learner);

            //Act
            var response = await Sut().Refresh(_apprenticeshipIncentive);

            //Assert
            _mockLearnerService.Verify(m => m.Refresh(_apprenticeshipIncentive), Times.Never());
            response.Should().Be(learner);
        }

        [Test]
        public async Task Then_the_learner_is_refreshed_when_config_exists_and_a_refresh_is_due()
        {
            //Arrange
            _applicationSettings = new ApplicationSettings
            {
                LearnerServiceCacheIntervalInMinutes = "60"
            };

            var learner = Learner.Get(_fixture
                .Build<LearnerModel>()
                .Without(l => l.LearningPeriods)
                .With(l => l.RefreshDate, DateTime.UtcNow.AddMinutes(-61))
                .Create());

            _mockDateTimeService
                .Setup(m => m.UtcNow())
                .Returns(DateTime.UtcNow);

            _mockLearnerDomainRepository
               .Setup(m => m.Get(_apprenticeshipIncentive))
               .ReturnsAsync(learner);

            //Act
            var response = await Sut().Refresh(_apprenticeshipIncentive);

            //Assert
            _mockLearnerService.Verify(m => m.Refresh(_apprenticeshipIncentive), Times.Once());
            response.Should().Be(_refreshedLearner);
        }

        [Test]
        public async Task Then_the_learner_is_refreshed_when_config_exists_and_a_learner_does_not_exist()
        {
            //Arrange
            _applicationSettings = new ApplicationSettings
            {
                LearnerServiceCacheIntervalInMinutes = "60"
            };
           
            _mockDateTimeService
                .Setup(m => m.UtcNow())
                .Returns(DateTime.UtcNow);

            _mockLearnerDomainRepository
               .Setup(m => m.Get(_apprenticeshipIncentive))
               .ReturnsAsync((Learner)null);

            //Act
            var response = await Sut().Refresh(_apprenticeshipIncentive);

            //Assert
            _mockLearnerService.Verify(m => m.Refresh(_apprenticeshipIncentive), Times.Once());
            response.Should().Be(_refreshedLearner);
        }

        [Test]
        public async Task Then_the_learner_is_refreshed_when_config_exists_and_a_learner_has_a_null_LastRefreshed_date()
        {
            //Arrange
            _applicationSettings = new ApplicationSettings
            {
                LearnerServiceCacheIntervalInMinutes = "60"
            };

            var learner = Learner.Get(_fixture
                .Build<LearnerModel>()
                .Without(l => l.LearningPeriods)
                .Without(l => l.RefreshDate)
                .Create());

            _mockDateTimeService
                .Setup(m => m.UtcNow())
                .Returns(DateTime.UtcNow);

            _mockLearnerDomainRepository
               .Setup(m => m.Get(_apprenticeshipIncentive))
               .ReturnsAsync(learner);

            //Act
            var response = await Sut().Refresh(_apprenticeshipIncentive);

            //Assert
            _mockLearnerService.Verify(m => m.Refresh(_apprenticeshipIncentive), Times.Once());
            response.Should().Be(_refreshedLearner);
        }
    }
}
