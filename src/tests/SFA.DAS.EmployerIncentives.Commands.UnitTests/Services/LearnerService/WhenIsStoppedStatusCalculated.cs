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
        private List<CollectionCalendarPeriod> _collectionPeriods;
        private DateTime _censusDate;

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

            _academicYears = new List<AcademicYear>
            {
                new AcademicYear("2021", new DateTime(2021, 07, 31)),
                new AcademicYear("2122", new DateTime(2022, 07, 31))
            };
            _censusDate = new DateTime(2021, 07, 31);
            _collectionPeriods = new List<CollectionCalendarPeriod>
            {
                new CollectionCalendarPeriod(new Domain.ValueObjects.CollectionPeriod(12, 2021), 7, 2021, new DateTime(2021, 07, 01), _censusDate, true, false)
            };
            _collectionCalendar = new Domain.ValueObjects.CollectionCalendar(_academicYears, _collectionPeriods);
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
        public void Then_isStopped_is_false_is_returned_when_the_latest_price_episode_end_date_is_the_census_date_of_the_active_period()
        {
            //Arrange    
            _testPriceEpisodeDto.EndDate = _censusDate;

            // Act
            var isStoppedStatus = _sut.IsStopped(_incentive, _collectionCalendar);

            //Assert
            isStoppedStatus.LearningStopped.Should().BeFalse();
            isStoppedStatus.DateStopped.HasValue.Should().BeFalse();
            isStoppedStatus.DateResumed.Value.Should().Be(_testPriceEpisodeDto.StartDate);
        }

        [Test]
        public void Then_isStopped_is_false_is_returned_when_the_latest_price_episode_end_date_is_after_the_census_date_of_the_active_period()
        {
            //Arrange    
            _testPriceEpisodeDto.EndDate = _censusDate.AddDays(1);

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
            _testPriceEpisodeDto.EndDate = _academicYears.First().EndDate;

            // Act
            var isStoppedStatus = _sut.IsStopped(_incentive, _collectionCalendar);

            //Assert
            isStoppedStatus.LearningStopped.Should().BeFalse();
            isStoppedStatus.DateStopped.HasValue.Should().BeFalse();
            isStoppedStatus.DateResumed.Value.Should().Be(_testPriceEpisodeDto.StartDate);
        }

        [Test]
        public void Then_isStopped_is_true_is_returned_when_the_latest_price_episode_end_date_is_before_the_census_date_of_the_active_period()
        {
            //Arrange    
            _testPriceEpisodeDto.EndDate = GetValidPastEndDate(_censusDate.AddDays(-1));

            // Act
            var isStoppedStatus = _sut.IsStopped(_incentive, _collectionCalendar);

            //Assert
            isStoppedStatus.LearningStopped.Should().BeTrue();
            isStoppedStatus.DateStopped.HasValue.Should().BeTrue();
            isStoppedStatus.DateResumed.HasValue.Should().BeFalse();
        }

        [Test]
        public void Then_isStopped_stopped_date_is_the_day_after_the_end_date_when_the_latest_price_episode_end_date_is_before_the_census_date_of_the_active_period()
        {
            //Arrange    
            _testPriceEpisodeDto.EndDate = GetValidPastEndDate(_censusDate.AddDays(-10));

            // Act
            var isStoppedStatus = _sut.IsStopped(_incentive, _collectionCalendar);

            //Assert
            isStoppedStatus.LearningStopped.Should().BeTrue();
            isStoppedStatus.DateStopped.Should().Be(_testPriceEpisodeDto.EndDate.Value.AddDays(1));
            isStoppedStatus.DateResumed.HasValue.Should().BeFalse();
        }

        [Test] // https://skillsfundingagency.atlassian.net/browse/EI-1403
        public void Then_isStopped_is_recognised_when_latest_training_has_no_price_episodes()
        {
            // Arrange
            _sut.Training = new List<TrainingDto>
            {
                _fixture
                    .Build<TrainingDto>()
                    .With(p => p.Reference, "ZPROG001")
                    .With(p => p.PriceEpisodes, new List<PriceEpisodeDto>
                        {
                            _fixture.Build<PriceEpisodeDto>()
                                .With(x => x.AcademicYear, "2122")
                                .With(x => x.StartDate, DateTime.Parse("2021-08-01T00:00:00"))
                                .With(x => x.EndDate, DateTime.Parse("2021-08-15T00:00:00"))
                                .With(pe => pe.Periods, new List<PeriodDto>())
                                .Create(),
                            _fixture.Build<PriceEpisodeDto>()
                                .With(x => x.AcademicYear, "2021")
                                .With(x => x.StartDate, DateTime.Parse("2021-04-15T00:00:00"))
                                .With(x => x.EndDate, DateTime.Parse("2021-07-31T00:00:00"))
                                .With(pe => pe.Periods, new List<PeriodDto>
                                {
                                    _fixture.Build<PeriodDto>()
                                        .With(period => period.ApprenticeshipId, _incentive.Apprenticeship.Id)
                                        .With(period => period.IsPayable, true)
                                        .With(period => period.Period, 9)
                                        .Create(),
                                    _fixture.Build<PeriodDto>()
                                        .With(period => period.ApprenticeshipId, _incentive.Apprenticeship.Id)
                                        .With(period => period.IsPayable, true)
                                        .With(period => period.Period, 10)
                                        .Create(),
                                    _fixture.Build<PeriodDto>()
                                        .With(period => period.ApprenticeshipId, _incentive.Apprenticeship.Id)
                                        .With(period => period.IsPayable, true)
                                        .With(period => period.Period, 11)
                                        .Create(),
                                    _fixture.Build<PeriodDto>()
                                        .With(period => period.ApprenticeshipId, _incentive.Apprenticeship.Id)
                                        .With(period => period.IsPayable, true)
                                        .With(period => period.Period, 12)
                                        .Create()
                                })
                                .Create()
                        }
                    )
                    .Create()
            };

            // Act
            var isStoppedStatus = _sut.IsStopped(_incentive, _collectionCalendar);

            // Assert
            isStoppedStatus.LearningStopped.Should().BeTrue();
            var endDateOfLatestValidPriceEpisode = _sut.Training.Single().PriceEpisodes.First(pe => pe.Periods.Any()).EndDate.Value;
            isStoppedStatus.DateStopped.Should().Be(endDateOfLatestValidPriceEpisode.AddDays(1));
            isStoppedStatus.DateResumed.HasValue.Should().BeFalse();
        }

        [Test] // https://skillsfundingagency.atlassian.net/browse/EI-1403
        public void Then_isStopped_is_true_and_stop_date_is_a_day_after_last_period_end_date_when_there_is_no_end_date_supplied()
        {
            // Arrange
            _sut.Training = new List<TrainingDto>
            {
                _fixture
                    .Build<TrainingDto>()
                    .With(p => p.Reference, "ZPROG001")
                    .With(p => p.PriceEpisodes, new List<PriceEpisodeDto>
                        {
                            _fixture.Build<PriceEpisodeDto>()
                                .With(x => x.AcademicYear, "2122")
                                .With(x => x.StartDate, DateTime.Parse("2021-08-01T00:00:00"))
                                .Without(x => x.EndDate)
                                .With(pe => pe.Periods, new List<PeriodDto>())
                                .Create(),
                            _fixture.Build<PriceEpisodeDto>()
                                .With(x => x.AcademicYear, "2021")
                                .With(x => x.StartDate, DateTime.Parse("2021-04-15T00:00:00"))
                                .Without(x => x.EndDate)
                                .With(pe => pe.Periods, new List<PeriodDto>
                                {
                                    _fixture.Build<PeriodDto>()
                                        .With(period => period.ApprenticeshipId, _incentive.Apprenticeship.Id)
                                        .With(period => period.IsPayable, true)
                                        .With(period => period.Period, 9)
                                        .Create(),
                                    _fixture.Build<PeriodDto>()
                                        .With(period => period.ApprenticeshipId, _incentive.Apprenticeship.Id)
                                        .With(period => period.IsPayable, true)
                                        .With(period => period.Period, 10)
                                        .Create(),
                                    _fixture.Build<PeriodDto>()
                                        .With(period => period.ApprenticeshipId, _incentive.Apprenticeship.Id)
                                        .With(period => period.IsPayable, true)
                                        .With(period => period.Period, 11)
                                        .Create(),
                                    _fixture.Build<PeriodDto>()
                                        .With(period => period.ApprenticeshipId, _incentive.Apprenticeship.Id)
                                        .With(period => period.IsPayable, true)
                                        .With(period => period.Period, 12)
                                        .Create()
                                })
                                .Create()
                        }
                    )
                    .Create()
            };

            // Act
            var isStoppedStatus = _sut.IsStopped(_incentive, _collectionCalendar);

            // Assert
            isStoppedStatus.LearningStopped.Should().BeTrue();
            isStoppedStatus.DateStopped.Should().Be(_censusDate.AddDays(1));
            isStoppedStatus.DateResumed.HasValue.Should().BeFalse();
        }

        private DateTime GetValidPastEndDate(DateTime endDate)
        {
            return endDate.Date == _academicYears.First().EndDate.Date ? endDate.AddDays(-1) : endDate;
        }
    }
}
