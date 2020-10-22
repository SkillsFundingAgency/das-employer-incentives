using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests
{
    public class WhenCreatePayment
    {
        private ApprenticeshipIncentive _sut;
        private ApprenticeshipIncentiveModel _sutModel;
        private short _collectionYear;
        private byte _collectionMonth;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _collectionYear = _fixture.Create<short>();
            _collectionMonth = _fixture.Create<byte>();

            _sutModel = _fixture.Build<ApprenticeshipIncentiveModel>().With(x => x.PaymentModels, new List<PaymentModel>()).Create();
            _sut = Sut(_sutModel);
        }

        [Test]
        public void Then_an_exception_is_thrown_when_the_pending_payment_does_not_exist()
        {
            // act
            Action result = () => _sut.CreatePayment(Guid.NewGuid(), _collectionYear, _collectionMonth);

            // assert
            result.Should().Throw<ArgumentException>().WithMessage("Pending payment does not exist.");            
        }

        [Test]
        public void Then_the_payment_is_not_created_when_the_pending_payment_is_not_valid()
        {
            //TODO
        }

        [Test]
        public void Then_the_payment_is_created()
        {
            // arrange
            var pendingPayment = _sut.PendingPayments.First();

            // act
            _sut.CreatePayment(pendingPayment.Id, _collectionYear, _collectionMonth);

            // assert
            _sut.Payments.Count.Should().Be(1);
            var actualPayment = _sut.Payments.First();
            actualPayment.Account.Id.Should().Be(_sut.Account.Id);
            actualPayment.Account.AccountLegalEntityId.Should().Be(_sut.Account.AccountLegalEntityId);
            actualPayment.Amount.Should().Be(pendingPayment.Amount);
            actualPayment.PaymentPeriod.Should().Be(_collectionMonth);
            actualPayment.PaymentYear.Should().Be(_collectionYear);
        }

        [Test]
        public void Then_the_payment_is_created_and_the_existing_payment_is_replaced()
        {
            // arrange
            var pendingPayment = _sut.PendingPayments.First();
            var existingPayment = _fixture.Build<PaymentModel>().With(x => x.PendingPaymentId, pendingPayment.Id).Create();
            _sutModel.PaymentModels.Add(existingPayment);

                // act
            _sut.CreatePayment(pendingPayment.Id, _collectionYear, _collectionMonth);

            // assert
            _sut.Payments.Count.Should().Be(1);
            _sut.Payments.First().Id.Should().NotBe(existingPayment.Id);
        }

        [Test]
        public void Then_the_pending_payment_is_updated_with_the_payment_made_date()
        {
            // arrange
            var pendingPayment = _sut.PendingPayments.First();

            // act
            _sut.CreatePayment(pendingPayment.Id, _collectionYear, _collectionMonth);

            // assert
            pendingPayment.PaymentMadeDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
        }

        private ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentive.Get(model.Id, model);
        }
    }
}
