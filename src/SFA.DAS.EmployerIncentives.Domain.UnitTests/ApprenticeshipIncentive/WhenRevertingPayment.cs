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
    public class WhenRevertingPayment
    {
        private ApprenticeshipIncentives.ApprenticeshipIncentive _sut;
        private ApprenticeshipIncentiveModel _sutModel;
        private Fixture _fixture;
        private Guid _pendingPaymentId1;
        private Guid _pendingPaymentId2;
        private Guid _paymentId;
        private ServiceRequest _serviceRequest;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _pendingPaymentId1 = Guid.NewGuid();
            _pendingPaymentId2 = Guid.NewGuid();
            _paymentId = Guid.NewGuid();
            _serviceRequest = _fixture.Create<ServiceRequest>();
        }

        [Test]
        public void Then_the_first_payment_is_removed_and_the_pending_payment_marked_unpaid()
        {
            // Arrange
            _sutModel = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .With(x => x.Status, IncentiveStatus.Active)
                .With(x => x.Phase, new IncentivePhase(Phase.Phase3))
                .With(x => x.StartDate, new DateTime(2021, 11, 01))
                .With(a => a.PendingPaymentModels, new List<PendingPaymentModel>
                {
                    _fixture.Build<PendingPaymentModel>()
                        .With(x => x.PaymentMadeDate, _fixture.Create<DateTime>())
                        .With(x => x.Id, _pendingPaymentId1)
                        .With(x => x.EarningType, EarningType.FirstPayment)
                        .Create(),
                    _fixture.Build<PendingPaymentModel>()
                        .Without(x => x.PaymentMadeDate)
                        .With(x => x.EarningType, EarningType.SecondPayment)
                        .With(x => x.Id, _pendingPaymentId2)
                        .Create()
                })
                .With(a => a.PaymentModels, new List<PaymentModel>
                {
                    _fixture.Build<PaymentModel>()
                        .With(x => x.Id, _paymentId)
                        .With(x => x.PendingPaymentId, _pendingPaymentId1)
                        .With(x => x.PaidDate, _fixture.Create<DateTime>())
                        .Create()
                })
                .Create();

            _sut = Sut(_sutModel);

            // Act
            _sut.RevertPayment(_paymentId, _serviceRequest);

            // Assert
            _sut.Payments.Count.Should().Be(0);
            _sut.PendingPayments.Count.Should().Be(2);
            _sut.PendingPayments.FirstOrDefault(x => x.EarningType == EarningType.FirstPayment).PaymentMadeDate.Should().BeNull();
            _sut.PendingPayments.FirstOrDefault(x => x.EarningType == EarningType.SecondPayment).PaymentMadeDate.Should().BeNull();
        }

        [Test]
        public void Then_the_second_payment_is_removed_and_the_pending_payment_marked_unpaid()
        {
            // Arrange
            _sutModel = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .With(x => x.Status, IncentiveStatus.Active)
                .With(x => x.Phase, new IncentivePhase(Phase.Phase3))
                .With(x => x.StartDate, new DateTime(2021, 11, 01))
                .With(a => a.PendingPaymentModels, new List<PendingPaymentModel>
                {
                    _fixture.Build<PendingPaymentModel>()
                        .With(x => x.PaymentMadeDate, _fixture.Create<DateTime>())
                        .With(x => x.Id, _pendingPaymentId1)
                        .With(x => x.EarningType, EarningType.FirstPayment)
                        .Create(),
                    _fixture.Build<PendingPaymentModel>()
                        .Without(x => x.PaymentMadeDate)
                        .With(x => x.EarningType, EarningType.SecondPayment)
                        .With(x => x.Id, _pendingPaymentId2)
                        .Create()
                })
                .With(a => a.PaymentModels, new List<PaymentModel>
                {
                    _fixture.Build<PaymentModel>()
                        .With(x => x.Id, Guid.NewGuid())
                        .With(x => x.PendingPaymentId, _pendingPaymentId1)
                        .With(x => x.PaidDate, _fixture.Create<DateTime>())
                        .Create(),
                    _fixture.Build<PaymentModel>()
                        .With(x => x.Id, _paymentId)
                        .With(x => x.PendingPaymentId, _pendingPaymentId2)
                        .With(x => x.PaidDate, _fixture.Create<DateTime>())
                        .Create()
                })
                .Create();

            _sut = Sut(_sutModel);

            // Act
            _sut.RevertPayment(_paymentId, _serviceRequest);

            // Assert
            _sut.Payments.Count.Should().Be(1);
            _sut.PendingPayments.Count.Should().Be(2);
            _sut.PendingPayments.FirstOrDefault(x => x.EarningType == EarningType.FirstPayment).PaymentMadeDate.Should().NotBeNull();
            _sut.PendingPayments.FirstOrDefault(x => x.EarningType == EarningType.SecondPayment).PaymentMadeDate.Should().BeNull();
        }

        private ApprenticeshipIncentives.ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentives.ApprenticeshipIncentive.Get(model.Id, model);
        }
    }
}
