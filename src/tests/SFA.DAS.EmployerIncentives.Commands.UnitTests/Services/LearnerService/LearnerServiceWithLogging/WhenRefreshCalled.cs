using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Application.UnitTests;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.Services.LearnerService.LearnerServiceWithLoggingTests
{
    public class WhenRefreshCalled
    {
        private LearnerServiceWithLogging _sut;        
        private Mock<ILearnerService> _mockLearnerService;
        private Mock<ILogger<Learner>> _mockLogger;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _mockLearnerService = new Mock<ILearnerService>();
            _mockLogger = new Mock<ILogger<Learner>>();

            _sut = new LearnerServiceWithLogging(_mockLearnerService.Object, _mockLogger.Object);
        }


        [Test]
        public async Task Then_the_start_of_the_call_is_logged()
        {
            //Arrange
            var learner = _fixture.Create<Learner>();

            //Act
            await _sut.Refresh(learner);

            //Assert
            _mockLogger.VerifyLog(LogLevel.Information, Times.Once(), $"Start refresh of learner data from learner match service for ApprenticeshipIncentiveId : {learner.ApprenticeshipIncentiveId},  ApprenticeshipId : {learner.ApprenticeshipId}, Ukprn : {learner.Ukprn}, Url : {learner.UniqueLearnerNumber}");
        }

        [Test]
        public async Task Then_the_end_of_the_call_is_logged()
        {
            //Arrange
            var learner = _fixture.Create<Learner>();

            //Act
            await _sut.Refresh(learner);

            //Assert
            _mockLogger.VerifyLog(LogLevel.Information, Times.Once(), $"Learner data refresh completed for ApprenticeshipIncentiveId : {learner.ApprenticeshipIncentiveId},  ApprenticeshipId : {learner.ApprenticeshipId}, Ukprn : {learner.Ukprn}, Url : {learner.UniqueLearnerNumber} with result SubmissionFound : {learner.SubmissionFound}");
        }

        [Test]
        public async Task Then_the_learner_is_passed_to_the_service()
        {
            //Arrange
            var learner = _fixture.Create<Learner>();

            //Act
            await _sut.Refresh(learner);

            //Assert
            _mockLearnerService.Verify(m => m.Refresh(learner), Times.Once);
        }

        [Test]
        public void Then_a_service_exception_is_logged()
        {
            //Arrange
            var learner = _fixture.Create<Learner>();
            var exception = new Exception();

            _mockLearnerService
                .Setup(m => m.Refresh(learner))
                .ThrowsAsync(exception);

            //Act
            Func<Task> action = async () => await _sut.Refresh(learner);
            action.Invoke();

            //Assert
            _mockLogger.VerifyLog(LogLevel.Error, Times.Once(), $"Error during learner data refresh for ApprenticeshipIncentiveId : {learner.ApprenticeshipIncentiveId},  ApprenticeshipId : {learner.ApprenticeshipId}, Ukprn : {learner.Ukprn}, Url : {learner.UniqueLearnerNumber} with result SubmissionFound : {learner.SubmissionFound}", exception);
        }

        [Test]
        public void Then_the_service_exception_is_propogated()
        {
            //Arrange
            var learner = _fixture.Create<Learner>();
            var errorMessage = _fixture.Create<string>();

            _mockLearnerService
                .Setup(m => m.Refresh(learner))
                .ThrowsAsync(new Exception(errorMessage));

            //Act
            Func<Task> action = async () => await _sut.Refresh(learner);

            //Assert
            action.Should().Throw<Exception>().WithMessage(errorMessage);
        }
    }
}
