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
    public class WhenDaysInLearningCalculated
    {
        private LearnerSubmissionDto _sut;
        private TrainingDto _testTrainingDto;
        private PriceEpisodeDto _testPriceEpisode1Dto;
        private PriceEpisodeDto _testPriceEpisode2Dto;
        private PriceEpisodeDto _testPriceEpisode3Dto;
        private DateTime _startDate;
        private Domain.ApprenticeshipIncentives.ApprenticeshipIncentive _incentive;
        private ApprenticeshipIncentiveModel _apprenticeshipIncentiveModel;
        private CollectionCalendar _collectionCalendar;
        private DateTime _censusDate;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();                                   

            _sut = _fixture.Create<LearnerSubmissionDto>();            
            _startDate = DateTime.Now.Date;
            _censusDate = _startDate.AddDays(17);

            _collectionCalendar = new CollectionCalendar(new List<CollectionPeriod>()
            {
                new CollectionPeriod(1, (byte)DateTime.Now.Month, (short)DateTime.Now.Year, DateTime.Now.AddMonths(-2), _censusDate, DateTime.Now.Year.ToString(), true )
            });

            var dueDate = _startDate.AddMonths(1);

            var pendingPaymentModel = _fixture
                .Build<PendingPaymentModel>()
                .With(pp => pp.PaymentMadeDate, (DateTime?)null)
                .With(p => p.DueDate, dueDate)
                .Create();
            _apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(p => p.Apprenticeship, _fixture.Create<Apprenticeship>())
                .With(p => p.PendingPaymentModels, new List<PendingPaymentModel> {
                     _fixture.Build<PendingPaymentModel>()
                    .With(pp => pp.PaymentMadeDate, (DateTime?)null)
                    .With(pp => pp.DueDate, dueDate.AddMonths(1))
                    .With(pp => pp.PaymentYear, (short?)null)
                    .Create(),
                    _fixture.Build<PendingPaymentModel>()
                    .With(pp => pp.PaymentMadeDate, (DateTime?)null)
                    .With(pp => pp.DueDate, dueDate.AddMonths(2))
                    .With(pp => pp.PaymentYear, (short?)null)
                    .Create(),
                    pendingPaymentModel
                    })
                .Create();

            _incentive = new ApprenticeshipIncentiveFactory().GetExisting(_apprenticeshipIncentiveModel.Id, _apprenticeshipIncentiveModel);

            _testTrainingDto = _sut.Training.First();
            _testTrainingDto.Reference = "ZPROG001";

            _testTrainingDto.PriceEpisodes.Clear();

            _testPriceEpisode1Dto = _fixture.Create<PriceEpisodeDto>();
            _testPriceEpisode1Dto.StartDate = _startDate.AddDays(-60);
            _testPriceEpisode1Dto.EndDate = _testPriceEpisode1Dto.StartDate.AddDays(15);
            _testPriceEpisode1Dto.Periods.First().ApprenticeshipId = _apprenticeshipIncentiveModel.Apprenticeship.Id;
            _testTrainingDto.PriceEpisodes.Add(_testPriceEpisode1Dto);

            _testPriceEpisode2Dto = _fixture.Create<PriceEpisodeDto>();
            _testPriceEpisode2Dto.StartDate = _startDate.AddDays(-30);
            _testPriceEpisode2Dto.EndDate = _testPriceEpisode2Dto.StartDate.AddDays(12);
            _testPriceEpisode2Dto.Periods.First().ApprenticeshipId = _apprenticeshipIncentiveModel.Apprenticeship.Id;
            _testTrainingDto.PriceEpisodes.Add(_testPriceEpisode2Dto);

            _testTrainingDto.PriceEpisodes.Add(_fixture.Create<PriceEpisodeDto>());

            _testPriceEpisode3Dto = _fixture.Create<PriceEpisodeDto>();
            _testPriceEpisode3Dto.StartDate = _startDate;
            _testPriceEpisode3Dto.EndDate = null;
            _testPriceEpisode3Dto.Periods.First().ApprenticeshipId = _apprenticeshipIncentiveModel.Apprenticeship.Id;
            _testTrainingDto.PriceEpisodes.Add(_testPriceEpisode3Dto);
        }


        [Test]
        public void Then_expected_days_is_returned_when_there_are_matching_periods_and_null_latest_price_episode_end_date()
        {
            //Arrange  
            int expectedDays = 17 + 15 + 12;

            //Act
            var daysInLearning = _sut.DaysInLearning(_incentive, _collectionCalendar);

            //Assert
            daysInLearning.Should().Be(expectedDays);
        }

        [Test]
        public void Then_expected_days_is_returned_when_there_are_matching_periods_and_the_census_date_is_before_latest_price_episode_end_date()
        {
            //Arrange  
            _testPriceEpisode3Dto.EndDate = _censusDate.AddDays(1);
            int expectedDays = 17 + 15 + 12;

            //Act
            var daysInLearning = _sut.DaysInLearning(_incentive, _collectionCalendar);

            //Assert
            daysInLearning.Should().Be(expectedDays);
        }

        [Test]
        public void Then_expected_days_is_returned_when_there_are_matching_periods_and_the_census_date_is_after_latest_price_episode_end_date()
        {
            //Arrange  
            _testPriceEpisode3Dto.EndDate = _censusDate.AddDays(-1);
            int expectedDays = 17 + 15 + 12 - 1;

            //Act
            var daysInLearning = _sut.DaysInLearning(_incentive, _collectionCalendar);

            //Assert
            daysInLearning.Should().Be(expectedDays);
        }

        [Test]
        public void Then_null_is_returned_when_there_is_no_matching_training_records()
        {
            //Arrange    
            _testTrainingDto.Reference = _fixture.Create<string>();

            //Act
            var daysInLearning = _sut.DaysInLearning(_incentive, _collectionCalendar);

            //Assert
            daysInLearning.Should().BeNull();
        }

        [Test]
        public void Then_null_is_returned_when_there_is_no_matching_periods_for_the_apprenticeship()
        {
            //Arrange    
            _testPriceEpisode1Dto.Periods.First().ApprenticeshipId = _apprenticeshipIncentiveModel.Apprenticeship.Id + 1;
            _testPriceEpisode2Dto.Periods.First().ApprenticeshipId = _apprenticeshipIncentiveModel.Apprenticeship.Id + 1;
            _testPriceEpisode3Dto.Periods.First().ApprenticeshipId = _apprenticeshipIncentiveModel.Apprenticeship.Id + 1;

            //Act
            var daysInLearning = _sut.DaysInLearning(_incentive, _collectionCalendar);

            //Assert
            daysInLearning.Should().BeNull();
        }
    }
}
