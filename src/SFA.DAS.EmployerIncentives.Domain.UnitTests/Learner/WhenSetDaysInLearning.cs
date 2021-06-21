using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.LearnerTests
{
    public class WhenSetDaysInLearning
    {
        private Learner _sut;
        private LearnerModel _sutModel;
        private CollectionCalendarPeriod _collectionCalendarPeriod;
        private DateTime _censusDate;
        private DateTime _startDate;
        private LearningPeriod _learningPeriod3;
        private List<LearningPeriod> _learningPeriods;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _startDate = DateTime.Now.Date;
            _censusDate = _startDate.AddDays(17);

            _collectionCalendarPeriod = new CollectionCalendarPeriod(new CollectionPeriod(1, (short)DateTime.Now.Year), (byte)DateTime.Now.Month, (short)DateTime.Now.Year, DateTime.Now.AddMonths(-2), _censusDate, true);

            _learningPeriod3 = new LearningPeriod(_startDate, null);
            _learningPeriods = new List<LearningPeriod>()
            {
                new LearningPeriod(_startDate.AddDays(-60), _startDate.AddDays(-60 + 15)),
                new LearningPeriod(_startDate.AddDays(-30), _startDate.AddDays(-30 + 12)),
                _learningPeriod3
            };

            _sutModel = _fixture
                .Build<LearnerModel>()
                .With(l => l.LearningPeriods, _learningPeriods)
                .With(l => l.DaysInLearnings, new List<DaysInLearning>())
                .Create();

            _sut = Sut(_sutModel);
        }

        [Test()]
        public void Then_days_in_learning_is_created_with_the_expected_number_of_days_when_the_last_LearningPeriod_end_date_is_null()
        {
            // arrange 
            int expectedDays = 18 + 16 + 13;

            // act
            _sut.SetDaysInLearning(_collectionCalendarPeriod);

            // assert            
            var daysInLearning = _sut.GetModel().DaysInLearnings.Single();
            daysInLearning.CollectionPeriod.Should().Be(_collectionCalendarPeriod.CollectionPeriod);
            daysInLearning.NumberOfDays.Should().Be(expectedDays);
        }

        [Test()]
        public void Then_days_in_learning_is_created_with_the_expected_number_of_days_when_the_collection_period_census_date_is_before_the_last_LearningPeriod_end_date()
        {
            // arrange 
            _learningPeriod3 = new LearningPeriod(_learningPeriod3.StartDate, _censusDate.AddDays(1));
            _learningPeriods = new List<LearningPeriod>()
            {
                new LearningPeriod(_startDate.AddDays(-60), _startDate.AddDays(-60 + 15)),
                new LearningPeriod(_startDate.AddDays(-30), _startDate.AddDays(-30 + 12)),
                _learningPeriod3
            };
            _sutModel = _fixture
               .Build<LearnerModel>()
               .With(l => l.LearningPeriods, _learningPeriods)
               .With(l => l.DaysInLearnings, new List<DaysInLearning>())
               .Create();

            int expectedDays = 18 + 16 + 13;
            _sut = Sut(_sutModel);

            // act
            _sut.SetDaysInLearning(_collectionCalendarPeriod);

            // assert            
            var daysInLearning = _sut.GetModel().DaysInLearnings.Single();
            daysInLearning.CollectionPeriod.Should().Be(_collectionCalendarPeriod.CollectionPeriod);
            daysInLearning.NumberOfDays.Should().Be(expectedDays);
        }

        [Test()]
        public void Then_days_in_learning_is_created_with_the_expected_number_of_days_when_the_collection_period_census_date_is_after_the_last_LearningPeriod_end_date()
        {
            // arrange 
            _learningPeriod3 = new LearningPeriod(_learningPeriod3.StartDate, _censusDate.AddDays(-1));
            _learningPeriods = new List<LearningPeriod>()
            {
                new LearningPeriod(_startDate.AddDays(-60), _startDate.AddDays(-60 + 15)),
                new LearningPeriod(_startDate.AddDays(-30), _startDate.AddDays(-30 + 12)),
                _learningPeriod3
            };
            _sutModel = _fixture
               .Build<LearnerModel>()
               .With(l => l.LearningPeriods, _learningPeriods)
               .With(l => l.DaysInLearnings, new List<DaysInLearning>())
               .Create();            

            int expectedDays = 18 + 16 + 13 - 1;
            _sut = Sut(_sutModel);

            // act
            _sut.SetDaysInLearning(_collectionCalendarPeriod);

            // assert            
            var daysInLearning = _sut.GetModel().DaysInLearnings.Single();
            daysInLearning.CollectionPeriod.Should().Be(_collectionCalendarPeriod.CollectionPeriod);
            daysInLearning.NumberOfDays.Should().Be(expectedDays);
        }

        [Test()]
        public void Then_days_in_learning_is_zero_if_there_are_no_LearningPeriods()
        {
            // arrange             
            _sutModel = _fixture
               .Build<LearnerModel>()
               .With(l => l.LearningPeriods, new List<LearningPeriod>())
               .With(l => l.DaysInLearnings, new List<DaysInLearning>())
               .Create();

            int expectedDays = 0;
            _sut = Sut(_sutModel);

            // act
            _sut.SetDaysInLearning(_collectionCalendarPeriod);

            // assert            
            var daysInLearning = _sut.GetModel().DaysInLearnings.Single();
            daysInLearning.CollectionPeriod.Should().Be(_collectionCalendarPeriod.CollectionPeriod);
            daysInLearning.NumberOfDays.Should().Be(expectedDays);
        }

        [Test()]
        public void Then_the_existing_daya_in_learning_is_updated_for_the_collection_period()
        {
            // arrange             
            _sutModel.DaysInLearnings.Add(new DaysInLearning(_collectionCalendarPeriod.CollectionPeriod, 5));

            int expectedDays = 18 + 16 + 13;
            _sut = Sut(_sutModel);

            // act
            _sut.SetDaysInLearning(_collectionCalendarPeriod);

            // assert            
            var daysInLearning = _sut.GetModel().DaysInLearnings.Single();
            daysInLearning.CollectionPeriod.Should().Be(_collectionCalendarPeriod.CollectionPeriod);
            daysInLearning.NumberOfDays.Should().Be(expectedDays);
        }

        private Learner Sut(LearnerModel model)
        {
            return Learner.Get(model);
        }
    }
}
