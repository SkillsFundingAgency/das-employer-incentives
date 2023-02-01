using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests
{
    [TestFixture]
    public class WhenRecalculateEarnings
    {
        private ApprenticeshipIncentives.ApprenticeshipIncentive _sut;
        private ApprenticeshipIncentiveModel _sutModel;
        private Fixture _fixture;
        private List<CollectionCalendarPeriod> _collectionPeriods;
        private CollectionCalendar _collectionCalendar;
        private DateTime _collectionPeriod;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _collectionPeriod = new DateTime(2020, 10, 1);
            _collectionPeriods = new List<CollectionCalendarPeriod>()
            {
                new CollectionCalendarPeriod(new CollectionPeriod(1, _fixture.Create<short>()), (byte)_collectionPeriod.AddMonths(-1).Month, (short)_collectionPeriod.AddMonths(-1).Year, _fixture.Create<DateTime>(), _collectionPeriod.AddMonths(1).AddDays(1), true, false)
            };
            for (var i = 1; i <= 12; i++)
            {
                _collectionPeriods.Add(new CollectionCalendarPeriod(new CollectionPeriod((byte)i, _fixture.Create<short>()), (byte)_collectionPeriod.AddMonths(i).Month, (short)_collectionPeriod.AddMonths(i).Year, _fixture.Create<DateTime>(), _collectionPeriod.AddMonths(i + 1).AddDays(1), false, false)
                );
            }

            _collectionCalendar = new CollectionCalendar(new List<AcademicYear>(), _collectionPeriods);
        }

        [Test]
        public void Then_both_existing_earnings_are_recalculated_if_no_payments_exist()
        {
            // Arrange
            _sutModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(x => x.Phase, new IncentivePhase(Phase.Phase3))
                .With(x => x.Status, IncentiveStatus.Active)
                .With(x => x.StartDate, new DateTime(2021, 09, 30))
                .Create();

            _sutModel.PendingPaymentModels = new List<PendingPaymentModel>(
                _fixture.Build<PendingPaymentModel>()
                    .Without(x => x.PaymentMadeDate)
                    .CreateMany(2));

            _sut = Sut(_sutModel);

            // Act
            _sut.ReCalculateEarnings(_collectionCalendar);

            // Assert
            _sut.GetModel().PendingPaymentModels.Count.Should().Be(0);
        }

        [TestCase(IncentiveStatus.Withdrawn)]
        [TestCase(IncentiveStatus.Stopped)]
        public void Then_the_earnings_are_not_recalculated_when_the_status_is_invalid(IncentiveStatus status)
        {
            // Arrange
            _sutModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(x => x.Phase, new IncentivePhase(Phase.Phase3))
                .With(x => x.Status, status)
                .With(x => x.PreviousStatus, status)
                .With(x => x.PendingPaymentModels, new List<PendingPaymentModel>(
                    _fixture.Build<PendingPaymentModel>()
                        .Without(x => x.PaymentMadeDate)
                        .CreateMany(2)))
                .Create();
            
            _sut = Sut(_sutModel);

            // Act
            _sut.ReCalculateEarnings(_collectionCalendar);

            // Assert
            _sut.GetModel().PendingPaymentModels.Count.Should().Be(2);
        }

        [Test]
        public void Then_the_earnings_are_not_recalculated_if_payments_already_exist()
        {
            // Arrange
            _sutModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(x => x.Phase, new IncentivePhase(Phase.Phase3))
                .With(x => x.Status, IncentiveStatus.Active)
                .Create();
            _sutModel.PendingPaymentModels = new List<PendingPaymentModel>(
                _fixture.Build<PendingPaymentModel>()
                    .With(x => x.PaymentMadeDate, DateTime.Now.AddDays(-1))
                    .CreateMany(2));

            var payments = new List<PaymentModel>();
            foreach(var pendingPayment in _sutModel.PendingPaymentModels)
            {
                var payment = _fixture.Build<PaymentModel>()
                    .With(x => x.PendingPaymentId, pendingPayment.Id)
                    .With(x => x.PaidDate, DateTime.Now.AddDays(-1))
                    .Create();
                payments.Add(payment);
            }
            _sutModel.PaymentModels = payments;

            _sut = Sut(_sutModel);

            // Act
            _sut.ReCalculateEarnings(_collectionCalendar);

            // Assert
            _sut.GetModel().PendingPaymentModels.Count.Should().Be(2);
        }

        [Test]
        public void Then_the_earnings_are_not_recalculated_if_a_payment_exists_for_the_first_earning()
        {
            // Arrange
            _sutModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(x => x.Phase, new IncentivePhase(Phase.Phase3))
                .With(x => x.Status, IncentiveStatus.Active)
                .Create();

            var pendingPayments = new List<PendingPaymentModel>();
            var pendingPayment1 = _fixture.Build<PendingPaymentModel>()
                    .With(x => x.PaymentMadeDate, DateTime.Now.AddDays(-1))
                    .Create();
            pendingPayments.Add(pendingPayment1);
            var pendingPayment2 = _fixture.Build<PendingPaymentModel>()
                .Without(x => x.PaymentMadeDate)
                .Create();
            pendingPayments.Add(pendingPayment2);

            _sutModel.PendingPaymentModels = pendingPayments;
                
            var payments = new List<PaymentModel>();
            var payment1 = _fixture.Build<PaymentModel>()
                .With(x => x.PendingPaymentId, _sutModel.PendingPaymentModels.ToList()[0].Id)
                .With(x => x.PaidDate, DateTime.Now.AddDays(-1))
                .Create();
            payments.Add(payment1);
            _sutModel.PaymentModels = payments;

            _sut = Sut(_sutModel);

            // Act
            _sut.ReCalculateEarnings(_collectionCalendar);

            // Assert
            _sut.GetModel().PendingPaymentModels.Count.Should().Be(2);
            
        }

        private ApprenticeshipIncentives.ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentives.ApprenticeshipIncentive.Get(model.Id, model);
        }
    }
}
