using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using System;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.Services.LearnerServiceTests
{
    public class WhenLearningStartDateForApprenticeship
    {
        private LearnerSubmissionDto _sut;
        private TrainingDto _testTrainingDto;
        private PriceEpisodeDto _testPriceEpisodeDto;
        private PeriodDto _testPeriodDto;
        private Domain.ApprenticeshipIncentives.ApprenticeshipIncentive _incentive;
        private DateTime _startTime;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var model = _fixture.Create<ApprenticeshipIncentiveModel>();
            _incentive = new ApprenticeshipIncentiveFactory().GetExisting(model.Id, model);

            _sut = _fixture.Create<LearnerSubmissionDto>();
            _startTime = DateTime.Now;

            _testTrainingDto = _sut.Training.First();
            _testTrainingDto.Reference = "ZPROG001";

            _testPriceEpisodeDto = _testTrainingDto.PriceEpisodes.First();
            _testPriceEpisodeDto.StartDate = _startTime;

            _testPeriodDto = _testPriceEpisodeDto.Periods.First();

            _testPeriodDto.ApprenticeshipId = _incentive.Apprenticeship.Id;
            _testPeriodDto.IsPayable = true;
        }


        [Test]
        public void Then_the_start_date_is_set_when_there_is_a_matching_record()
        {
            //Arrange
            

            //Act
            var startDate = _sut.LearningStartDateForApprenticeship(_incentive);

            //Assert
            startDate.Should().Be(_startTime);
        }

        [Test]
        public void Then_the_start_date_is_null_when_there_are_no_payable_periods()
        {
            //Arrange
            _testPriceEpisodeDto.Periods.ToList().ForEach(p => p.IsPayable = false);

            //Act
            var startDate = _sut.LearningStartDateForApprenticeship(_incentive);

            //Assert
            startDate.Should().BeNull();
        }

        [Test]
        public void Then_the_start_date_is_null_when_there_are_no_matching_training_records()
        {
            //Arrange
            _testTrainingDto.Reference = _fixture.Create<string>();

            //Act
            var startDate = _sut.LearningStartDateForApprenticeship(_incentive);

            //Assert
            startDate.Should().BeNull();
        }

        [Test]
        public void Then_the_start_date_is_null_when_the_are_periods_for_the_apprenticeship()
        {
            //Arrange
            _testPeriodDto.ApprenticeshipId = _fixture.Create<long>();

            //Act
            var startDate = _sut.LearningStartDateForApprenticeship(_incentive);

            //Assert
            startDate.Should().BeNull();
        }

        [Test]
        public void Then_the_start_date_is_the_earliest_matching_period()
        {
            //Arrange
            var counter = 1;
            _testTrainingDto.PriceEpisodes.ToList().ForEach(pe => 
            {
                pe.Periods.ToList().ForEach(p => { p.ApprenticeshipId = _incentive.Apprenticeship.Id; p.IsPayable = true; });            
                pe.StartDate = _startTime.AddDays(counter*-1); counter++; 
            });

            //Act
            var startDate = _sut.LearningStartDateForApprenticeship(_incentive);

            //Assert
            startDate.Should().Be(_startTime.AddDays(-3));
        }
    }
}
