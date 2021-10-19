using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;
using System.Collections.Generic;
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
        private AcademicYear _academicYear;
        private Domain.ValueObjects.CollectionCalendar _collectionCalendar;
        private long _apprenticeshipId;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sut = _fixture.Create<LearnerSubmissionDto>();

            _apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(p => p.Apprenticeship, _fixture.Create<Apprenticeship>())
                .Create();

            _incentive = new ApprenticeshipIncentiveFactory().GetExisting(_apprenticeshipIncentiveModel.Id, _apprenticeshipIncentiveModel);
            _apprenticeshipId = _apprenticeshipIncentiveModel.Apprenticeship.Id;

            _testTrainingDto = _sut.Training.First();
            _testTrainingDto.Reference = "ZPROG001";

            _testTrainingDto.PriceEpisodes.Clear();

            var periods = new List<PeriodDto> {new PeriodDto {ApprenticeshipId = _apprenticeshipId}};
            _testPriceEpisode1Dto = _fixture.Build<PriceEpisodeDto>()
                .With(pe => pe.AcademicYear, "2021")
                .With(pe => pe.StartDate, new DateTime(2021, 1, 1))
                .With(pe => pe.EndDate, new DateTime(2021, 3, 1))
                .With(pe => pe.Periods, periods)
                .Create();
            _testTrainingDto.PriceEpisodes.Add(_testPriceEpisode1Dto);

            _testPriceEpisode2Dto = _fixture.Build<PriceEpisodeDto>()
                .With(pe => pe.AcademicYear, "2021")
                .With(pe => pe.StartDate, new DateTime(2021, 4, 1))
                .With(pe => pe.EndDate, new DateTime(2021, 6, 1))
                .With(pe => pe.Periods, periods)
                .Create();
            _testTrainingDto.PriceEpisodes.Add(_testPriceEpisode2Dto);

            _testTrainingDto.PriceEpisodes.Add(_fixture.Create<PriceEpisodeDto>()); // non matching

            _testPriceEpisode3Dto = _fixture.Build<PriceEpisodeDto>()// duplicate
                .With(pe => pe.AcademicYear, "2021")
                .With(pe => pe.StartDate, _testPriceEpisode1Dto.StartDate)
                .With(pe => pe.EndDate, _testPriceEpisode1Dto.EndDate)
                .With(pe => pe.Periods, periods)
                .Create();
            _testTrainingDto.PriceEpisodes.Add(_testPriceEpisode3Dto);

            _academicYear = new AcademicYear("2021", new DateTime(2021, 7, 31));
            _collectionCalendar = new Domain.ValueObjects.CollectionCalendar(new List<AcademicYear> { _academicYear }, new List<CollectionCalendarPeriod>());
        }

        [Test]
        public void Then_expected_periods_are_returned_when_there_are_price_episodes_with_periods_for_the_apprenticeship()
        {
            //Arrange              

            //Act
            var learningPeriods = _sut.LearningPeriods(_incentive, _collectionCalendar);

            //Assert
            learningPeriods.Count().Should().Be(2);
            learningPeriods.Single(e => e.StartDate == _testPriceEpisode1Dto.StartDate && e.EndDate == _testPriceEpisode1Dto.EndDate);
            learningPeriods.Single(e => e.StartDate == _testPriceEpisode2Dto.StartDate && e.EndDate == _testPriceEpisode2Dto.EndDate);
        }

        [Test]
        public void Then_null_end_dates_are_set_to_the_end_of_the_academic_year()
        {
            //Arrange
            _testPriceEpisode2Dto.EndDate = null;

            //Act
            var learningPeriods = _sut.LearningPeriods(_incentive, _collectionCalendar);

            //Assert
            learningPeriods.Count().Should().Be(2);
            learningPeriods.Single(e => e.StartDate == _testPriceEpisode1Dto.StartDate && e.EndDate == _testPriceEpisode1Dto.EndDate);
            learningPeriods.Single(e => e.StartDate == _testPriceEpisode2Dto.StartDate && e.EndDate == _academicYear.EndDate);
        }

        [Test]
        public void Then_an_empty_collection_is_returned_if_the_learner_data_is_null()
        {
            //Arrange  

            //Act
            var learningPeriods = LearnerDataExtensions.LearningPeriods(null, _incentive, _collectionCalendar);

            //Assert
            learningPeriods.Count().Should().Be(0);
        }

        [Test]
        public void Then_gap_of_28_days_or_less_are_not_considered_as_breaks()
        {
            //Arrange  
            var periods = new List<PeriodDto> {new PeriodDto {ApprenticeshipId = _apprenticeshipId}};
            var episode = _fixture.Build<PriceEpisodeDto>()
                .With(x => x.StartDate, _testPriceEpisode3Dto.EndDate.Value.AddDays(28))
                .With(x => x.AcademicYear, _testPriceEpisode3Dto.AcademicYear)
                .With(x => x.Periods, periods)
                .Without(x => x.EndDate)
                .Create();
            _sut.Training.First().PriceEpisodes.Add(episode);

            //Act
            var learningPeriods = _sut.LearningPeriods(_incentive, _collectionCalendar);

            //Assert
            learningPeriods.Count.Should().Be(2);
            learningPeriods.Last().StartDate.Should().Be(_testPriceEpisode3Dto.StartDate);
            learningPeriods.Last().EndDate.Should().BeNull();
        }

        [Test]
        public void Then_gaps_of_28_days_or_less_are_not_considered_as_breaks()
        {
            //Arrange  
            var periods = new List<PeriodDto> { new PeriodDto { ApprenticeshipId = _apprenticeshipId } };
            var episode1 = _fixture.Build<PriceEpisodeDto>()
                .With(x => x.StartDate, _testPriceEpisode1Dto.StartDate.AddDays(-100))
                .With(x => x.EndDate, _testPriceEpisode1Dto.StartDate.AddDays(-28))
                .With(x => x.AcademicYear, _testPriceEpisode3Dto.AcademicYear)
                .With(x => x.Periods, periods)
                .Create();
            _sut.Training.First().PriceEpisodes.Add(episode1);

            var episode2 = _fixture.Build<PriceEpisodeDto>()
                .With(x => x.StartDate, _testPriceEpisode3Dto.EndDate.Value.AddDays(28))
                .With(x => x.AcademicYear, _testPriceEpisode3Dto.AcademicYear)
                .With(x => x.Periods, periods)
                .Without(x => x.EndDate)
                .Create();
            _sut.Training.First().PriceEpisodes.Add(episode2);

            //Act
            var learningPeriods = _sut.LearningPeriods(_incentive, _collectionCalendar);

            //Assert
            learningPeriods.Count.Should().Be(2);
        }

        [Test]
        public void Then_overlapping_periods_are_merged()
        {
            //Arrange  
            var periods = new List<PeriodDto> { new PeriodDto { ApprenticeshipId = _apprenticeshipId } };
            var episode = _fixture.Build<PriceEpisodeDto>()
                .With(x => x.StartDate, _testPriceEpisode1Dto.StartDate.AddDays(30))
                .With(x => x.EndDate, _testPriceEpisode1Dto.StartDate.AddDays(-30))
                .With(x => x.Periods, periods)
                .Create();
            _sut.Training.First().PriceEpisodes.Add(episode);
   
            //Act
            var learningPeriods = _sut.LearningPeriods(_incentive, _collectionCalendar);

            //Assert
            learningPeriods.Count().Should().Be(2);
        }
    }
}
