using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.LearnerTests
{
    public class WhenGetBreakInLearningDayCount
    {
        private Learner _sut;
        private LearnerModel _sutModel;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sutModel = _fixture
                .Build<LearnerModel>()
                .Create();            
        }

        [Test()]
        public void Then_break_in_learning_day_count_is_zero_when_there_are_no_learning_periods()
        {
            // arrange 
            _sutModel.LearningPeriods = new List<LearningPeriod>();
            _sut = Sut(_sutModel);

            // act
            var days = _sut.GetBreakInLearningDayCount();

            // assert            
            days.Should().Be(0);
        }

        [Test()]
        public void Then_break_in_learning_day_count_is_zero_when_the_is_only_one_learning_period_with_a_null_end_date()
        {
            // arrange 
            _sutModel.LearningPeriods = new List<LearningPeriod>
            {
                new LearningPeriod(_fixture.Create<DateTime>(), null)
            };

            _sut = Sut(_sutModel);

            // act
            var days = _sut.GetBreakInLearningDayCount();

            // assert            
            days.Should().Be(0);
        }

        [Test()]
        public void Then_break_in_learning_day_count_is_zero_when_there_is_no_break_in_learning()
        {
            // arrange 
            _sutModel.LearningPeriods = new List<LearningPeriod>
            {
                new LearningPeriod(new DateTime(2020, 11, 12), new DateTime(2020, 12, 4) ),
                new LearningPeriod(new DateTime(2020, 12, 5), new DateTime(2021, 1, 17) ),
                new LearningPeriod(new DateTime(2021, 1, 18), null)
            };

            _sut = Sut(_sutModel);

            // act
            var days = _sut.GetBreakInLearningDayCount();

            // assert            
            days.Should().Be(0);
        }

        [Test()]
        public void Then_break_in_learning_day_count_is_set_when_there_is_a_break_in_learning()
        {
            // arrange 
            _sutModel.LearningPeriods = new List<LearningPeriod>
            {
                new LearningPeriod(new DateTime(2020, 11, 12), new DateTime(2020, 12, 4) ),
                new LearningPeriod(new DateTime(2021, 1, 18), null)
            };

            _sut = Sut(_sutModel);

            // act
            var days = _sut.GetBreakInLearningDayCount();

            // assert            
            days.Should().Be(44);
        }

        [Test()]
        public void Then_the_cumulative_break_in_learning_day_count_is_set_when_there_are_multiple_break_in_learnings()
        {
            // arrange 
            _sutModel.LearningPeriods = new List<LearningPeriod>
            {
                new LearningPeriod(new DateTime(2020, 11, 12), new DateTime(2020, 12, 4) ),
                new LearningPeriod(new DateTime(2021, 1, 18), new DateTime(2021, 2, 22)),
                new LearningPeriod(new DateTime(2021, 3, 05), new DateTime(2021, 4, 15))
            };

            _sut = Sut(_sutModel);

            // act
            var days = _sut.GetBreakInLearningDayCount();

            // assert            
            days.Should().Be(54);
        }

        [Test()]
        public void Then_the_cumulative_break_in_learning_day_count_is_set_when_there_are_multiple_break_in_learnings_ordered_by_start_date()
        {
            // arrange 
            _sutModel.LearningPeriods = new List<LearningPeriod>
            {
                new LearningPeriod(new DateTime(2021, 3, 05), new DateTime(2021, 4, 15)),
                new LearningPeriod(new DateTime(2020, 11, 12), new DateTime(2020, 12, 4) ),
                new LearningPeriod(new DateTime(2021, 1, 18), new DateTime(2021, 2, 22))
            };

            _sut = Sut(_sutModel);

            // act
            var days = _sut.GetBreakInLearningDayCount();

            // assert            
            days.Should().Be(54);
        }
       
        private Learner Sut(LearnerModel model)
        {
            return Learner.Get(model);
        }
    }
}
