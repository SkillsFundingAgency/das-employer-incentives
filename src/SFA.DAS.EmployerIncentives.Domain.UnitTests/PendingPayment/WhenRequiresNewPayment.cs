using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests
{
    [TestFixture]
    public class WhenRequiresNewPayment
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
                .With(p => p.CollectionPeriod, new CollectionPeriod((byte)1, (short)2021))
                .With(p => p.EarningType, EarningType.FirstPayment)
                .Without(p => p.PaymentMadeDate)
                .With(p => p.ClawedBack, false)
                .Create();

            _newPendingPaymentModel =
                _fixture
                .Build<PendingPaymentModel>()
                .With(p => p.Account, _sutModel.Account)
                .With(p => p.ApprenticeshipIncentiveId, _sutModel.ApprenticeshipIncentiveId)
                .With(p => p.Amount, _sutModel.Amount)
                .With(p => p.CollectionPeriod, new CollectionPeriod(_sutModel.CollectionPeriod.PeriodNumber, _sutModel.CollectionPeriod.AcademicYear))
                .Without(p => p.PaymentMadeDate)
                .With(p => p.ClawedBack, false)
                .Create();

            _newPendingPayment = PendingPayment.Get(_newPendingPaymentModel);

            _sut = PendingPayment.Get(_sutModel);
        }

        [Test]
        public void Then_is_false_when_there_are_no_changes()
        {
            // arrange            

            // act
            var result = _sut.RequiresNewPayment(_newPendingPayment);

            // assert
            result.Should().BeFalse();
        }

        [Test]
        public void Then_is_true_when_the_amount_change()
        {
            // arrange            

            _newPendingPaymentModel.Amount = _sut.Amount + 1;
            _newPendingPayment = PendingPayment.Get(_newPendingPaymentModel);

            // act
            var result = _sut.RequiresNewPayment(_newPendingPayment);

            // assert
            result.Should().BeTrue();
        }

        [Test]
        public void Then_is_true_when_the_period_changes()
        {
            // arrange            

            _newPendingPaymentModel.CollectionPeriod = new CollectionPeriod((byte)(_sut.CollectionPeriod.PeriodNumber + 1), _sut.CollectionPeriod.AcademicYear);
            _newPendingPayment = PendingPayment.Get(_newPendingPaymentModel);

            // act
            var result = _sut.RequiresNewPayment(_newPendingPayment);

            // assert
            result.Should().BeTrue();
        }
        

        [Test]
        public void Then_is_true_when_the_payment_year_changes()
        {
            // arrange            

            _newPendingPaymentModel.CollectionPeriod = new CollectionPeriod(_sut.CollectionPeriod.PeriodNumber, (short)(_sut.CollectionPeriod.AcademicYear +  1));
            _newPendingPayment = PendingPayment.Get(_newPendingPaymentModel);

            // act
            var result = _sut.RequiresNewPayment(_newPendingPayment);

            // assert
            result.Should().BeTrue();
        }
    }
}
