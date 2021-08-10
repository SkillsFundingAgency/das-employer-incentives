using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.Services.LearnerServiceTests
{
    public class WhenIsStoppedStatusCalculated
    {
        private LearnerSubmissionDto _sut;
        private TrainingDto _testTrainingDto;
        private PriceEpisodeDto _testPriceEpisodeDto;
        private PeriodDto _testPeriodDto;
        private DateTime _startDate;
        private DateTime _endDate;
        private byte _periodNumber;
        private Domain.ApprenticeshipIncentives.ApprenticeshipIncentive _incentive;
        private ApprenticeshipIncentiveModel _apprenticeshipIncentiveModel;
        private Fixture _fixture;
        private List<AcademicYear> _academicYears;
        private Domain.ValueObjects.CollectionCalendar _collectionCalendar;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sut = _fixture.Create<LearnerSubmissionDto>();            
            _startDate = DateTime.Now.AddMonths(-1);
            _endDate = DateTime.Today.AddDays(-1);
            _periodNumber = 3;

            _apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(p => p.Apprenticeship, _fixture.Create<Apprenticeship>())                
                .Create();
            _incentive = new ApprenticeshipIncentiveFactory().GetExisting(_apprenticeshipIncentiveModel.Id, _apprenticeshipIncentiveModel);
            
            _testTrainingDto = _sut.Training.First();
            _testTrainingDto.Reference = "ZPROG001";

            _testPriceEpisodeDto = _testTrainingDto.PriceEpisodes.First();
            _testPriceEpisodeDto.StartDate = _startDate;
            _testPriceEpisodeDto.EndDate = _endDate;
            _testPriceEpisodeDto.AcademicYear = "2021";

            _testPeriodDto = _testPriceEpisodeDto.Periods.First();            

            _testPeriodDto.ApprenticeshipId = _apprenticeshipIncentiveModel.Apprenticeship.Id;
            _testPeriodDto.Period = _periodNumber;
            _testPeriodDto.IsPayable = true;

            _academicYears = new List<AcademicYear> {new AcademicYear("2021", new DateTime(2021, 07, 31))};
            _collectionCalendar = new Domain.ValueObjects.CollectionCalendar(_academicYears, new List<CollectionCalendarPeriod>());
        }

        [Test]
        public void Then_isStopped_true_is_returned_when_there_is_a_matching_period()
        {
            //Arrange    

            //Act
            var isStoppedStatus = _sut.IsStopped(_incentive, _collectionCalendar);

            //Assert
            isStoppedStatus.LearningStopped.Should().BeTrue();
        }

        [Test]
        public void Then_isStopped_is_false_is_returned_when_the_apprenticeship_is_null()
        {
            //Arrange    

            // Act
            var isStoppedStatus = _sut.IsStopped(null, _collectionCalendar);

            //Assert
            isStoppedStatus.LearningStopped.Should().BeFalse();
            isStoppedStatus.DateStopped.HasValue.Should().BeFalse();
            isStoppedStatus.DateResumed.HasValue.Should().BeFalse();
        }

        [Test]
        public void Then_isStopped_is_false_is_returned_when_the_apprenticeship_id_does_not_have_a_matching_period()
        {
            //Arrange    
            _testPeriodDto.ApprenticeshipId = _fixture.Create<long>();

            // Act
            var isStoppedStatus = _sut.IsStopped(_incentive, _collectionCalendar);

            //Assert
            isStoppedStatus.LearningStopped.Should().BeFalse();
            isStoppedStatus.DateStopped.HasValue.Should().BeFalse();
            isStoppedStatus.DateResumed.HasValue.Should().BeFalse();
        }

        [Test]
        public void Then_isStopped_is_false_is_returned_when_the_latest_price_episode_end_date_is_today()
        {
            //Arrange    
            _testPriceEpisodeDto.EndDate = DateTime.Today;

            // Act
            var isStoppedStatus = _sut.IsStopped(_incentive, _collectionCalendar);

            //Assert
            isStoppedStatus.LearningStopped.Should().BeFalse();
            isStoppedStatus.DateStopped.HasValue.Should().BeFalse();
            isStoppedStatus.DateResumed.Value.Should().Be(_testPriceEpisodeDto.StartDate);
        }

        [Test]
        public void Then_isStopped_is_false_is_returned_when_the_latest_price_episode_end_date_is_after_today()
        {
            //Arrange    
            _testPriceEpisodeDto.EndDate = DateTime.Today.AddDays(1);

            // Act
            var isStoppedStatus = _sut.IsStopped(_incentive, _collectionCalendar);

            //Assert
            isStoppedStatus.LearningStopped.Should().BeFalse();
            isStoppedStatus.DateStopped.HasValue.Should().BeFalse();            
            isStoppedStatus.DateResumed.Value.Should().Be(_testPriceEpisodeDto.StartDate);
        }

        [Test]
        public void Then_isStopped_is_false_is_returned_when_the_latest_price_episode_end_date_is_the_end_of_the_academic_year()
        {
            //Arrange    
            _testPriceEpisodeDto.EndDate = _academicYears.Single().EndDate;

            // Act
            var isStoppedStatus = _sut.IsStopped(_incentive, _collectionCalendar);

            //Assert
            isStoppedStatus.LearningStopped.Should().BeFalse();
            isStoppedStatus.DateStopped.HasValue.Should().BeFalse();
            isStoppedStatus.DateResumed.Value.Should().Be(_testPriceEpisodeDto.StartDate);
        }

        [Test]
        public void Then_isStopped_is_true_is_returned_when_the_latest_price_episode_end_date_is_before_today()
        {
            //Arrange    
            _testPriceEpisodeDto.EndDate = GetValidPastEndDate(DateTime.Today.AddDays(-1));

            // Act
            var isStoppedStatus = _sut.IsStopped(_incentive, _collectionCalendar);

            //Assert
            isStoppedStatus.LearningStopped.Should().BeTrue();
            isStoppedStatus.DateStopped.HasValue.Should().BeTrue();
            isStoppedStatus.DateResumed.HasValue.Should().BeFalse();
        }

        [Test]
        public void Then_isStopped_stopped_date_is_the_day_after_the_end_date_when_the_latest_price_episode_end_date_is_before_today()
        {
            //Arrange    
            _testPriceEpisodeDto.EndDate = GetValidPastEndDate(DateTime.Today.AddDays(-10));

            // Act
            var isStoppedStatus = _sut.IsStopped(_incentive, _collectionCalendar);

            //Assert
            isStoppedStatus.LearningStopped.Should().BeTrue();
            isStoppedStatus.DateStopped.Should().Be(_testPriceEpisodeDto.EndDate.Value.AddDays(1));
            isStoppedStatus.DateResumed.HasValue.Should().BeFalse();
        }

        private DateTime GetValidPastEndDate(DateTime endDate)
        {
            if (endDate.Date == _academicYears.Single().EndDate.Date)
            {
                return endDate.AddDays(-1);
            }

            return endDate;
        }
    }
}
