using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests
{
    public class WhenCalculateEarnings
    {
        private ApprenticeshipIncentive _sut;
        private ApprenticeshipIncentiveModel _sutModel;
        private Fixture _fixture;

        private List<IncentivePaymentProfile> _paymentProfiles;
        private List<CollectionPeriod> _collectionPeriods;
        private CollectionCalendar _collectionCalendar;
        private Apprenticeship _apprenticehip;
        private int _firstPaymentDaysAfterApprenticeshipStart;
        private int _secondPaymentDaysAfterApprenticeshipStart;
        private DateTime _collectionPeriod;
        private DateTime _plannedStartDate;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _plannedStartDate = DateTime.Now.AddDays(5);
            _collectionPeriod = DateTime.Now;

            _firstPaymentDaysAfterApprenticeshipStart = 10;
            _secondPaymentDaysAfterApprenticeshipStart = 50;

            var paymentProfiles = new List<PaymentProfile>
            {
                new PaymentProfile(_firstPaymentDaysAfterApprenticeshipStart, 100),
                new PaymentProfile(_secondPaymentDaysAfterApprenticeshipStart, 300)
            };

            _paymentProfiles = new List<IncentivePaymentProfile>()
            {
                new IncentivePaymentProfile(Enums.IncentiveType.UnderTwentyFiveIncentive, paymentProfiles)
            };

            _collectionPeriods = new List<CollectionPeriod>()
            {
                new CollectionPeriod(1, (byte)_collectionPeriod.AddMonths(1).Month, (short)_collectionPeriod.AddMonths(1).Year, _collectionPeriod.AddMonths(1).AddDays(1)),
                new CollectionPeriod(2, (byte)_collectionPeriod.Month, (short)_collectionPeriod.Year, _collectionPeriod.AddDays(1)),
                new CollectionPeriod(3, (byte)_collectionPeriod.AddMonths(-1).Month, (short)_collectionPeriod.AddMonths(-1).Year, _collectionPeriod.AddMonths(-1).AddDays(1))
            };

            _collectionCalendar = new CollectionCalendar(_collectionPeriods);

            _sutModel = _fixture.Create<ApprenticeshipIncentiveModel>();
            _apprenticehip = _sutModel.Apprenticeship;
            _sutModel.PlannedStartDate = _plannedStartDate;
            _sut = Sut(_sutModel);
        }

        [Test]
        public void Then_an_exception_is_thrown_when_the_incentive_does_not_pass_the_eligibility_check()
        {
            // arrange            
            var apprentiveshipDob = DateTime.Now.AddYears(-24);
            _sutModel.PlannedStartDate = Incentive.EligibilityStartDate.AddDays(-1);
            _sutModel.Apprenticeship = new Apprenticeship(_apprenticehip.Id, _apprenticehip.FirstName, _apprenticehip.LastName, apprentiveshipDob, _apprenticehip.UniqueLearnerNumber, _apprenticehip.EmployerType);            

            _sut = Sut(_sutModel);

            // act
            Action result = () => _sut.CalculateEarnings(_paymentProfiles, _collectionCalendar);

            // assert
            result.Should().Throw<InvalidIncentiveException>().WithMessage("Incentive does not pass the eligibility checks");            
        }

        [Test]
        public void Then_the_earnings_are_calculated_and_the_pending_payments_created()
        {
            // arrange                        

            // act
            _sut.CalculateEarnings(_paymentProfiles, _collectionCalendar);

            // assert
            _sut.PendingPayments.Count.Should().Be(2);

            var firstPayment = _sut.PendingPayments.First();
            var secondPayment = _sut.PendingPayments.Last();

            firstPayment.DueDate.Should().Be(_plannedStartDate.AddDays(_firstPaymentDaysAfterApprenticeshipStart));
            secondPayment.DueDate.Should().Be(_plannedStartDate.AddDays(_secondPaymentDaysAfterApprenticeshipStart));

            firstPayment.PeriodNumber.Should().Be(2);
            firstPayment.PaymentYear.Should().Be((short)_collectionPeriod.Year);
            firstPayment.Amount.Should().Be(100);
            secondPayment.PeriodNumber.Should().Be(1);
            secondPayment.PaymentYear.Should().Be((short)_collectionPeriod.AddMonths(1).Year);
            secondPayment.Amount.Should().Be(300);

            firstPayment.Account.Id.Should().Be(_sutModel.Account.Id);
            firstPayment.Account.AccountLegalEntityId.Should().Be(_sutModel.Account.AccountLegalEntityId);
            secondPayment.Account.Id.Should().Be(_sutModel.Account.Id);
            secondPayment.Account.AccountLegalEntityId.Should().Be(_sutModel.Account.AccountLegalEntityId);
        }

        [Test]
        public void Then_the_earnings_are_calculated_and_any_existing_pending_payments_are_replaced()
        {
            // Arrange
            _sutModel.PendingPaymentModels = new List<PendingPaymentModel>(_fixture.CreateMany<PendingPaymentModel>(3));
            _sut = Sut(_sutModel);
            _sut.PendingPayments.Count.Should().Be(3);
            var hashCodes = new List<int>();
            _sut.PendingPayments.ToList().ForEach(p => hashCodes.Add(p.GetHashCode()));

            // Act
            _sut.CalculateEarnings(_paymentProfiles, _collectionCalendar);

            // Assert
            _sut.PendingPayments.Count.Should().Be(2);
            _sut.PendingPayments.ToList().ForEach(p => hashCodes.Contains(p.GetHashCode()).Should().BeFalse());
        }

        [Test]
        public void Then_an_EarningsCalculated_event_is_raised_after_the_earnings_are_calculated()
        {
            // arrange                        
            _sut.CalculateEarnings(_paymentProfiles, _collectionCalendar);

            // act
            var events = _sut.FlushEvents();

            // assert
            var expectedEvent = events.Single() as EarningsCalculated;

            expectedEvent.ApprenticeshipIncentiveId.Should().Be(_sutModel.Id);
            expectedEvent.AccountId.Should().Be(_sutModel.Account.Id);
            expectedEvent.ApprenticeshipId.Should().Be(_sutModel.Apprenticeship.Id);
            expectedEvent.ApplicationApprenticeshipId.Should().Be(_sutModel.ApplicationApprenticeshipId);
        }

        private ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentive.Get(model.Id, model);
        }      

        [Test]
        public void Then_there_are_no_pending_payments()
        {
            // Arrange            

            // Act
            var incentive = new ApprenticeshipIncentiveFactory().CreateNew(_fixture.Create<Guid>(), _fixture.Create<Guid>(), _fixture.Create<ApprenticeshipIncentives.ValueTypes.Account>(), _fixture.Create<ApprenticeshipIncentives.ValueTypes.Apprenticeship>(), _fixture.Create<DateTime>());

            // Assert
            incentive.PendingPayments.Count.Should().Be(0);
            incentive.GetModel().PendingPaymentModels.Count.Should().Be(0);
        }
    }
}
