using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests
{
    [TestFixture]
    public class WhenRequiresNewPaymentAfterBreakInLearning
    {
        private PendingPayment _sut;
        private PendingPaymentModel _sutModel;
        private PendingPayment _newPendingPayment;
        private PendingPaymentModel _newPendingPaymentModel;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sutModel = _fixture
                .Build<PendingPaymentModel>()
                .With(p => p.DueDate, DateTime.Today)
                .With(p => p.CollectionPeriod, new CollectionPeriod(1, 2021))
                .With(p => p.EarningType, EarningType.FirstPayment)
                .Create();

            _newPendingPaymentModel =
                _fixture
                .Build<PendingPaymentModel>()
                .With(p => p.Account, _sutModel.Account)
                .With(p => p.ApprenticeshipIncentiveId, _sutModel.ApprenticeshipIncentiveId)
                .With(p => p.Amount, _sutModel.Amount)
                .With(p => p.CollectionPeriod, new CollectionPeriod(_sutModel.CollectionPeriod.PeriodNumber, _sutModel.CollectionPeriod.AcademicYear))
                .Create();

            _newPendingPayment = PendingPayment.Get(_newPendingPaymentModel);

            _sut = PendingPayment.Get(_sutModel);
        }

        [Test]
        public void Then_is_false_when_due_date_is_on_start_date_of_a_resumed_break_from_learning()
        {
            // arrange            
            var breakInLearning = BreakInLearning.Create(_sutModel.DueDate, _sutModel.DueDate.AddDays(10));
            var breakInLearnings = new List<BreakInLearning>()
            {
                breakInLearning
            };

            // act
            var result = _sut.RequiresNewPaymentAfterBreakInLearning(breakInLearnings);

            // assert
            result.Should().BeFalse();
        }

        [Test]
        public void Then_is_false_when_due_date_is_before_start_date_of_a_resumed_break_from_learning()
        {
            // arrange            
            var breakInLearning = BreakInLearning.Create(_sutModel.DueDate.AddDays(1), _sutModel.DueDate.AddDays(10));
            var breakInLearnings = new List<BreakInLearning>()
            {
                breakInLearning
            };

            // act
            var result = _sut.RequiresNewPaymentAfterBreakInLearning(breakInLearnings);

            // assert
            result.Should().BeFalse();
        }

        [Test]
        public void Then_is_true_when_due_date_is_before_start_date_of_a_stopped_break_from_learning_and_no_other_change()
        {
            // arrange
            var breakInLearnings = new List<BreakInLearning>()
            {
                new BreakInLearning(_sutModel.DueDate.AddDays(1))
            };

            // act
            var result = _sut.RequiresNewPaymentAfterBreakInLearning(breakInLearnings);

            // assert
            result.Should().BeTrue();
        }

        [Test]
        public void Then_is_true_when_there_are_no_stopped_break_from_learnings_and_no_other_change()
        {
            // arrange            
            var breakInLearnings = new List<BreakInLearning>();

            // act
            var result = _sut.RequiresNewPaymentAfterBreakInLearning(breakInLearnings);

            // assert
            result.Should().BeTrue();
        }
    }
}
