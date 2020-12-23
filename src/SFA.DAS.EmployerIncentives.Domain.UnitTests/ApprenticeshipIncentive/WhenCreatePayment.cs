using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests
{
    public class WhenCreatePayment
    {
        private ApprenticeshipIncentive _sut;
        private ApprenticeshipIncentiveModel _sutModel;
        private CollectionPeriod _collectionPeriod;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _collectionPeriod = _fixture.Create<CollectionPeriod>();
        
            _fixture.Customize<PendingPaymentValidationResultModel>(x => 
                x.With(y => y.Result, true)
                    .With(x=>x.CollectionPeriod, _collectionPeriod));
            _sutModel = _fixture.Build<ApprenticeshipIncentiveModel>().With(x => x.PaymentModels, new List<PaymentModel>()).Create();
            
            _sut = Sut(_sutModel);
        }

        [Test]
        public void Then_an_exception_is_thrown_when_the_pending_payment_does_not_exist()
        {
            // act
            Action result = () => _sut.CreatePayment(Guid.NewGuid(), _collectionPeriod.AcademicYear, _collectionPeriod.PeriodNumber);

            // assert
            result.Should().Throw<ArgumentException>().WithMessage("Pending payment does not exist.");            
        }

        [Test]
        public void Then_the_payment_is_not_created_when_the_pending_payment_is_not_valid()
        {
            var pendingPayment = _sutModel.PendingPaymentModels.First();
            pendingPayment.PendingPaymentValidationResultModels.First().Result = false;

            // act
            _sut.CreatePayment(pendingPayment.Id, _collectionPeriod.AcademicYear, _collectionPeriod.PeriodNumber);

            // assert
            _sut.Payments.Count.Should().Be(0);
        }

        [TestCase("2000-07-30", "2020-08-01", ApprenticeshipEmployerType.Levy, SubnominalCode.Levy19Plus)]
        [TestCase("2000-07-30", "2020-08-01", ApprenticeshipEmployerType.NonLevy, SubnominalCode.NonLevy19Plus)]
        [TestCase("2002-08-30", "2020-08-01", ApprenticeshipEmployerType.Levy, SubnominalCode.Levy16To18)]
        [TestCase("2002-08-30", "2020-08-01", ApprenticeshipEmployerType.NonLevy, SubnominalCode.NonLevy16To18)]
        [TestCase("2001-08-02", "2020-08-01", ApprenticeshipEmployerType.NonLevy, SubnominalCode.NonLevy16To18)]
        [TestCase("2001-08-01", "2020-08-01", ApprenticeshipEmployerType.NonLevy, SubnominalCode.NonLevy19Plus)]
        public void Then_the_payment_is_created(DateTime dob, DateTime plannedStartDate, ApprenticeshipEmployerType employerType, SubnominalCode expectedSubnominalCode)
        {
            // arrange
            var apprenticeship = new Apprenticeship(_fixture.Create<long>(), _fixture.Create<string>(),
                _fixture.Create<string>(), dob, _fixture.Create<long>(), employerType);

            _sutModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(x=>x.StartDate, plannedStartDate)
                .With(x=>x.Apprenticeship, apprenticeship)
                .With(x => x.PaymentModels, new List<PaymentModel>()).Create();

            _sut = Sut(_sutModel);

            var pendingPayment = _sut.PendingPayments.First();

            // act
            _sut.CreatePayment(pendingPayment.Id, _collectionPeriod.AcademicYear, _collectionPeriod.PeriodNumber);

            // assert
            _sut.Payments.Count.Should().Be(1);
            var actualPayment = _sut.Payments.First();
            actualPayment.SubnominalCode.Should().Be(expectedSubnominalCode);
        }

        [Test]
        public void Then_the_payment_is_created_with_correct_SubnominalCode()
        {
            // arrange
            var pendingPayment = _sut.PendingPayments.First();

            // act
            _sut.CreatePayment(pendingPayment.Id, _collectionPeriod.AcademicYear, _collectionPeriod.PeriodNumber);

            // assert
            _sut.Payments.Count.Should().Be(1);
            var actualPayment = _sut.Payments.First();
            actualPayment.Account.Id.Should().Be(_sut.Account.Id);
            actualPayment.Account.AccountLegalEntityId.Should().Be(_sut.Account.AccountLegalEntityId);
            actualPayment.Amount.Should().Be(pendingPayment.Amount);
            actualPayment.PaymentPeriod.Should().Be(_collectionPeriod.PeriodNumber);
            actualPayment.PaymentYear.Should().Be(_collectionPeriod.AcademicYear);
        }

        [Test]
        public void Then_the_payment_is_created_and_the_existing_payment_is_replaced()
        {
            // arrange
            var pendingPayment = _sut.PendingPayments.First();
            var existingPayment = _fixture.Build<PaymentModel>().With(x => x.PendingPaymentId, pendingPayment.Id).Create();
            _sutModel.PaymentModels.Add(existingPayment);

            // act
            _sut.CreatePayment(pendingPayment.Id, _collectionPeriod.AcademicYear, _collectionPeriod.PeriodNumber);

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
            _sut.CreatePayment(pendingPayment.Id, _collectionPeriod.AcademicYear, _collectionPeriod.PeriodNumber);

            // assert
            pendingPayment.PaymentMadeDate.Should().Be(DateTime.Today);
        }

        private ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentive.Get(model.Id, model);
        }
    }
}
