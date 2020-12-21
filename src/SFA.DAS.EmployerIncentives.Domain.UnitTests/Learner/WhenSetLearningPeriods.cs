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
    public class WhenSetLearningPeriods
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

            _sutModel.LearningPeriods = new List<LearningPeriod>();

            _sut = Sut(_sutModel);
        }

        [Test()]
        public void Then_learning_periods_are_created()
        {
            // arrange 
            var period1 = new LearningPeriod(_fixture.Create<DateTime>(), _fixture.Create<DateTime>());
            var period2 = new LearningPeriod(_fixture.Create<DateTime>(), null);
            var period3 = new LearningPeriod(_fixture.Create<DateTime>(), _fixture.Create<DateTime>());
            var leaningPeriods = new List<LearningPeriod>() {period1, period2, period3};

            // act
            _sut.SetLearningPeriods(leaningPeriods);

            // assert            
            var model = _sut.GetModel();
            model.LearningPeriods.Count.Should().Be(3);

            model.LearningPeriods.Single(p => p.StartDate == period1.StartDate && p.EndDate == period1.EndDate);
            model.LearningPeriods.Single(p => p.StartDate == period2.StartDate && p.EndDate == null);
            model.LearningPeriods.Single(p => p.StartDate == period3.StartDate && p.EndDate == period3.EndDate);
        }

        [Test()]
        public void Then_existing_periods_are_replaced()
        {
            // arrange 
            _sutModel.LearningPeriods.Add(new LearningPeriod(_fixture.Create<DateTime>(), _fixture.Create<DateTime>()));
            _sutModel.LearningPeriods.Add(new LearningPeriod(_fixture.Create<DateTime>(), _fixture.Create<DateTime>()));

            _sut = Sut(_sutModel);

            var period1 = new LearningPeriod(_fixture.Create<DateTime>(), _fixture.Create<DateTime>());
            var leaningPeriods = new List<LearningPeriod>() { period1};

            // act
            _sut.SetLearningPeriods(leaningPeriods);

            // assert            
            var model = _sut.GetModel();
            model.LearningPeriods.Count.Should().Be(1);

            model.LearningPeriods.Single(p => p.StartDate == period1.StartDate && p.EndDate == period1.EndDate);
        }

        private Learner Sut(LearnerModel model)
        {
            return Learner.Get(model);
        }
    }
}
