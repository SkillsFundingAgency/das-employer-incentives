using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.LearnerTests
{
    public class WhenGetDaysInLearning
    {
        private Learner _sut;
        private LearnerModel _sutModel;
        private DaysInLearning _daysInLearning;
        private CollectionPeriod _collectionPeriod;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sutModel = _fixture
                .Build<LearnerModel>()
                .Create();

            _collectionPeriod = _fixture.Create<CollectionPeriod>();

            _daysInLearning = new DaysInLearning(_collectionPeriod.PeriodNumber, _collectionPeriod.AcademicYear, _fixture.Create<int>());

            _sutModel.DaysInLearnings = new List<DaysInLearning>() {
                new DaysInLearning((byte)(_collectionPeriod.PeriodNumber - 1), (short)(_collectionPeriod.AcademicYear - 1), _fixture.Create<int>()),
                _daysInLearning,
                new DaysInLearning((byte)(_collectionPeriod.PeriodNumber + 1), (short)(_collectionPeriod.AcademicYear + 1), _fixture.Create<int>()),
            };

            _sut = Sut(_sutModel);
        }

        [Test()]
        public void Then_days_in_learning_for_the_collection_period_is_returned()
        {
            // arrange 

            // act
            var days = _sut.GetDaysInLearning(_collectionPeriod);

            // assert            
            days.Should().Be(_daysInLearning.NumberOfDays);
        }

        [Test()]
        public void Then_zero_days_is_returned_when_there_are_no_matching_days_in_learnings_for_the_collection_period()
        {
            // arrange 
            var collectionPeriod = new CollectionPeriod(
                (byte)(_collectionPeriod.PeriodNumber + 1),
                _collectionPeriod.CalendarMonth,
                _collectionPeriod.CalendarYear,
                _collectionPeriod.OpenDate,
                _collectionPeriod.CensusDate,
                _collectionPeriod.AcademicYear,
                _collectionPeriod.Active,
                _collectionPeriod.PeriodEndInProgress
                );

            // act
            int days = _sut.GetDaysInLearning(collectionPeriod);

            // assert            
            days.Should().Be(0);
        }

        private Learner Sut(LearnerModel model)
        {
            return Learner.Get(model);
        }
    }
}
