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
        private AcademicPeriod _academicPeriod;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sutModel = _fixture
                .Build<LearnerModel>()
                .Create();

            _academicPeriod = _fixture.Create<AcademicPeriod>();

            _daysInLearning = new DaysInLearning(_academicPeriod, _fixture.Create<int>());

            _sutModel.DaysInLearnings = new List<DaysInLearning>() {
                new DaysInLearning(new AcademicPeriod((byte)(_academicPeriod.PeriodNumber - 1), (short)(_academicPeriod.AcademicYear - 1)), _fixture.Create<int>()),
                _daysInLearning,
                new DaysInLearning(new AcademicPeriod((byte)(_academicPeriod.PeriodNumber + 1), (short)(_academicPeriod.AcademicYear + 1)), _fixture.Create<int>()),
            };

            _sut = Sut(_sutModel);
        }

        [Test()]
        public void Then_days_in_learning_for_the_collection_period_is_returned()
        {
            // arrange 

            // act
            var days = _sut.GetDaysInLearning(_academicPeriod);

            // assert            
            days.Should().Be(_daysInLearning.NumberOfDays);
        }

        [Test()]
        public void Then_zero_days_is_returned_when_there_are_no_matching_days_in_learnings_for_the_collection_period()
        {
            // arrange 
            var academicPeriod = new AcademicPeriod(
                (byte)(_academicPeriod.PeriodNumber + 1),
                _academicPeriod.AcademicYear
                );

            // act
            int days = _sut.GetDaysInLearning(academicPeriod);

            // assert            
            days.Should().Be(0);
        }

        private Learner Sut(LearnerModel model)
        {
            return Learner.Get(model);
        }
    }
}
