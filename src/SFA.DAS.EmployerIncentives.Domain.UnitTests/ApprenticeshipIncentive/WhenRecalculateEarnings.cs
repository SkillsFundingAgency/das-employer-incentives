using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
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
        private CollectionCalendar _collectionCalendar;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _collectionCalendar = _fixture.Create<CollectionCalendar>();
        }

        [Test]
        public void Then_the_existing_earnings_are_removed_and_a_calculate_earnings_event_is_triggered()
        {
            // Arrange
            _sutModel = _fixture.Create<ApprenticeshipIncentiveModel>();
            _sutModel.PendingPaymentModels = new List<PendingPaymentModel>(
                _fixture.Build<PendingPaymentModel>()
                    .Without(x => x.PaymentMadeDate)
                    .CreateMany(2));

            _sut = Sut(_sutModel);

            // Act
            _sut.RecalculateEarnings(_collectionCalendar);

            // Assert
            _sut.GetModel().PendingPaymentModels.Count.Should().Be(0);
            var events = _sut.FlushEvents().ToList();
            events.Should().ContainItemsAssignableTo<EarningsRecalculationRequired>();
        }

        [TestCase(IncentiveStatus.Withdrawn)]
        [TestCase(IncentiveStatus.Stopped)]
        public void Then_the_earnings_are_not_recalculated_when_the_status_is_invalid(IncentiveStatus status)
        {
            // Arrange
            _sutModel = _fixture.Create<ApprenticeshipIncentiveModel>();
            _sutModel.Status = status;
            _sutModel.PendingPaymentModels = new List<PendingPaymentModel>(
                _fixture.Build<PendingPaymentModel>()
                    .Without(x => x.PaymentMadeDate)
                    .CreateMany(2));

            _sut = Sut(_sutModel);

            // Act
            _sut.RecalculateEarnings(_collectionCalendar);

            // Assert
            _sut.GetModel().PendingPaymentModels.Count.Should().Be(2);
            var events = _sut.FlushEvents().ToList();
            events.Should().BeEmpty();
        }

        [Test]
        public void Then_the_earnings_are_not_recalculated_if_payments_already_exist()
        {
            // Arrange
            _sutModel = _fixture.Create<ApprenticeshipIncentiveModel>();
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
            _sut.RecalculateEarnings(_collectionCalendar);

            // Assert
            _sut.GetModel().PendingPaymentModels.Count.Should().Be(2);
            var events = _sut.FlushEvents().ToList();
            events.Should().BeEmpty();
        }

        [Test]
        public void Then_the_earnings_are_recalculated_if_a_payment_exists_for_the_first_earning()
        {
            // Arrange
            _sutModel = _fixture.Create<ApprenticeshipIncentiveModel>();

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
            _sut.RecalculateEarnings(_collectionCalendar);

            // Assert
            _sut.GetModel().PendingPaymentModels.Count.Should().Be(1);
            var events = _sut.FlushEvents().ToList();
            events.Should().ContainItemsAssignableTo<EarningsRecalculationRequired>();
        }

        private ApprenticeshipIncentives.ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentives.ApprenticeshipIncentive.Get(model.Id, model);
        }
    }
}
