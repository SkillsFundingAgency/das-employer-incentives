using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.Services.LearnerServiceTests
{
    public class WhenLearningPeriodsCalled
    {
        private LearnerSubmissionDto _sut;
        private TrainingDto _testTrainingDto;
        private PriceEpisodeDto _testPriceEpisode1Dto;
        private PriceEpisodeDto _testPriceEpisode2Dto;
        private PriceEpisodeDto _testPriceEpisode3Dto;
        private Domain.ApprenticeshipIncentives.ApprenticeshipIncentive _incentive;
        private ApprenticeshipIncentiveModel _apprenticeshipIncentiveModel;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sut = _fixture.Create<LearnerSubmissionDto>();

            _apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(p => p.Apprenticeship, _fixture.Create<Apprenticeship>())
                .Create();

            _incentive = new ApprenticeshipIncentiveFactory().GetExisting(_apprenticeshipIncentiveModel.Id, _apprenticeshipIncentiveModel);

            _testTrainingDto = _sut.Training.First();
            _testTrainingDto.Reference = "ZPROG001";

            _testTrainingDto.PriceEpisodes.Clear();

            _testPriceEpisode1Dto = _fixture.Create<PriceEpisodeDto>();
            _testPriceEpisode1Dto.Periods.First().ApprenticeshipId = _apprenticeshipIncentiveModel.Apprenticeship.Id;
            _testTrainingDto.PriceEpisodes.Add(_testPriceEpisode1Dto);

            _testPriceEpisode2Dto = _fixture.Create<PriceEpisodeDto>();
            _testPriceEpisode2Dto.EndDate = null;
            // probbaly not valid data but add multiple apprenticeship periods by price episode just in case
            _testPriceEpisode2Dto.Periods.ToList().ForEach(p => p.ApprenticeshipId = _apprenticeshipIncentiveModel.Apprenticeship.Id);
            _testTrainingDto.PriceEpisodes.Add(_testPriceEpisode2Dto);

            _testTrainingDto.PriceEpisodes.Add(_fixture.Create<PriceEpisodeDto>()); // non matching

            _testPriceEpisode3Dto = _fixture.Create<PriceEpisodeDto>(); // duplicate
            _testPriceEpisode3Dto.StartDate = _testPriceEpisode1Dto.StartDate;
            _testPriceEpisode3Dto.EndDate = _testPriceEpisode1Dto.EndDate;
            _testPriceEpisode3Dto.Periods.First().ApprenticeshipId = _apprenticeshipIncentiveModel.Apprenticeship.Id;
            _testTrainingDto.PriceEpisodes.Add(_testPriceEpisode3Dto);
        }

        [Test]
        public void Then_expected_periods_are_returned_when_there_are_price_episodes_with_periods_for_the_apprenticeship()
        {
            //Arrange              

            //Act
            var learningPeriods = _sut.LearningPeriods(_incentive);

            //Assert
            learningPeriods.Count().Should().Be(2);
            learningPeriods.Single(e => e.StartDate == _testPriceEpisode1Dto.StartDate && e.EndDate == _testPriceEpisode1Dto.EndDate);
            learningPeriods.Single(e => e.StartDate == _testPriceEpisode2Dto.StartDate && e.EndDate == null);
        }

        [Test]
        public void Then_an_empty_collection_is_returned_if_the_learner_data_is_null()
        {
            //Arrange  

            //Act
            var learningPeriods = LearnerDataExtensions.LearningPeriods(null, _incentive);

            //Assert
            learningPeriods.Count().Should().Be(0);
        }
    }
}
