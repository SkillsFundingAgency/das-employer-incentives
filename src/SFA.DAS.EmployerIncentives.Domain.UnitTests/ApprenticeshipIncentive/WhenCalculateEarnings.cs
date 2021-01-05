using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests
{
    public class WhenCalculateEarnings
    {
        private ApprenticeshipIncentive _sut;
        private ApprenticeshipIncentiveModel _sutModel;
        private Mock<ICollectionCalendarService> _mockCollectionCalendarService;
        private Mock<IIncentivePaymentProfilesService> _mockPaymentProfilesService;
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

            _collectionPeriod = new DateTime(2021, 1, 1);
            _plannedStartDate = _collectionPeriod.AddDays(5);

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
                new CollectionPeriod(1, (byte)_collectionPeriod.AddMonths(1).Month, (short)_collectionPeriod.AddMonths(1).Year, _collectionPeriod.AddMonths(1).AddDays(1), _fixture.Create<DateTime>(), _fixture.Create<short>(), false),
                new CollectionPeriod(2, (byte)_collectionPeriod.AddMonths(2).Month, (short)_collectionPeriod.AddMonths(2).Year, _collectionPeriod.AddMonths(2).AddDays(1), _fixture.Create<DateTime>(), _fixture.Create<short>(), false),
                new CollectionPeriod(3, (byte)_collectionPeriod.AddMonths(-1).Month, (short)_collectionPeriod.AddMonths(-1).Year, _collectionPeriod.AddMonths(-1).AddDays(1), _fixture.Create<DateTime>(), _fixture.Create<short>(), false)
            };

            _collectionCalendar = new CollectionCalendar(_collectionPeriods);

            _mockCollectionCalendarService = new Mock<ICollectionCalendarService>();
            _mockCollectionCalendarService.Setup(m => m.Get()).ReturnsAsync(_collectionCalendar);

            _mockPaymentProfilesService = new Mock<IIncentivePaymentProfilesService>();
            _mockPaymentProfilesService.Setup(m => m.Get()).ReturnsAsync(_paymentProfiles);

            _sutModel = _fixture.Create<ApprenticeshipIncentiveModel>();            
            _apprenticehip = _sutModel.Apprenticeship;
            _sutModel.StartDate = _plannedStartDate;
            _sutModel.PendingPaymentModels = new List<PendingPaymentModel>();
            _sut = Sut(_sutModel);
        }

        [Test]
        public async Task Then_earnings_are_not_calculated_when_the_incentive_does_not_pass_the_eligibility_check()
        {
            // arrange            
            var apprentiveshipDob = DateTime.Now.AddYears(-24);
            _sutModel.StartDate = Incentive.EligibilityStartDate.AddDays(-1);
            _sutModel.Apprenticeship = new Apprenticeship(_apprenticehip.Id, _apprenticehip.FirstName, _apprenticehip.LastName, apprentiveshipDob, _apprenticehip.UniqueLearnerNumber, _apprenticehip.EmployerType);
            
            _sut = Sut(_sutModel);

            // act
            await _sut.CalculateEarnings(_mockPaymentProfilesService.Object, _mockCollectionCalendarService.Object);

            // assert
            _sut.PendingPayments.Should().BeEmpty();
        }

        [Test]
        public async Task Then_the_earnings_are_calculated_and_the_pending_payments_created_using_the_planned_start_date_when_no_actual_start_date()
        {
            // arrange                        

            // act
            await _sut.CalculateEarnings(_mockPaymentProfilesService.Object, _mockCollectionCalendarService.Object);

            // assert
            _sut.PendingPayments.Count.Should().Be(2);

            var firstPayment = _sut.PendingPayments.First();
            var secondPayment = _sut.PendingPayments.Last();

            var endOfPlannedStartMonth = new DateTime(_plannedStartDate.Year, _plannedStartDate.Month, DateTime.DaysInMonth(_plannedStartDate.Year, _plannedStartDate.Month));

            firstPayment.DueDate.Should().Be(endOfPlannedStartMonth.AddDays(_firstPaymentDaysAfterApprenticeshipStart));
            secondPayment.DueDate.Should().Be(endOfPlannedStartMonth.AddDays(_secondPaymentDaysAfterApprenticeshipStart));

            firstPayment.PeriodNumber.Should().Be(1);
            firstPayment.PaymentYear.Should().Be(_collectionPeriods.Single(x => x.PeriodNumber == 1).AcademicYear);
            firstPayment.Amount.Should().Be(100);
            secondPayment.PeriodNumber.Should().Be(2);
            secondPayment.PaymentYear.Should().Be(_collectionPeriods.Single(x => x.PeriodNumber == 2).AcademicYear);
            secondPayment.Amount.Should().Be(300);

            firstPayment.Account.Id.Should().Be(_sutModel.Account.Id);
            firstPayment.Account.AccountLegalEntityId.Should().Be(_sutModel.Account.AccountLegalEntityId);
            secondPayment.Account.Id.Should().Be(_sutModel.Account.Id);
            secondPayment.Account.AccountLegalEntityId.Should().Be(_sutModel.Account.AccountLegalEntityId);

            firstPayment.EarningType.Should().Be(EarningType.FirstPayment);
            secondPayment.EarningType.Should().Be(EarningType.SecondPayment);
        }

        [Test]
        public async Task Then_the_earnings_are_calculated_and_the_pending_payments_created_using_the_actual_start_date_when_there_is_an_actual_start_date()
        {
            // arrange           
            _sutModel.StartDate = _collectionPeriod.Date.AddDays(6);

            // act
            await _sut.CalculateEarnings(_mockPaymentProfilesService.Object, _mockCollectionCalendarService.Object);

            // assert
            _sut.PendingPayments.Count.Should().Be(2);

            var firstPayment = _sut.PendingPayments.First();
            var secondPayment = _sut.PendingPayments.Last();

            var endOfPlannedStartMonth = new DateTime(_plannedStartDate.Year, _plannedStartDate.Month, DateTime.DaysInMonth(_plannedStartDate.Year, _plannedStartDate.Month));

            firstPayment.DueDate.Should().Be(endOfPlannedStartMonth.AddDays(_firstPaymentDaysAfterApprenticeshipStart));
            secondPayment.DueDate.Should().Be(endOfPlannedStartMonth.AddDays(_secondPaymentDaysAfterApprenticeshipStart));

            firstPayment.PeriodNumber.Should().Be(1);
            firstPayment.PaymentYear.Should().Be(_collectionPeriods.Single(x => x.PeriodNumber == 1).AcademicYear);
            firstPayment.Amount.Should().Be(100);
            secondPayment.PeriodNumber.Should().Be(2);
            secondPayment.PaymentYear.Should().Be(_collectionPeriods.Single(x => x.PeriodNumber == 2).AcademicYear);
            secondPayment.Amount.Should().Be(300);

            firstPayment.Account.Id.Should().Be(_sutModel.Account.Id);
            firstPayment.Account.AccountLegalEntityId.Should().Be(_sutModel.Account.AccountLegalEntityId);
            secondPayment.Account.Id.Should().Be(_sutModel.Account.Id);
            secondPayment.Account.AccountLegalEntityId.Should().Be(_sutModel.Account.AccountLegalEntityId);
        }

        [Test]
        public async Task Then_the_earnings_are_not_recalculated_if_earnings_already_exist()
        {
            // Arrange
            _sutModel.PendingPaymentModels = new List<PendingPaymentModel>(_fixture.CreateMany<PendingPaymentModel>(3));
            _sut = Sut(_sutModel);
            _sut.PendingPayments.Count.Should().Be(3);
            var hashCodes = new List<int>();
            _sut.PendingPayments.ToList().ForEach(p => hashCodes.Add(p.GetHashCode()));

            // Act
            await _sut.CalculateEarnings(_mockPaymentProfilesService.Object, _mockCollectionCalendarService.Object);

            // Assert
            _sut.PendingPayments.Count.Should().Be(3);
            _sut.PendingPayments.ToList().ForEach(p => hashCodes.Contains(p.GetHashCode()).Should().BeTrue());
        }

        [Test]
        public async Task Then_an_EarningsCalculated_event_is_raised_after_the_earnings_are_calculated()
        {
            // arrange                        
            await _sut.CalculateEarnings(_mockPaymentProfilesService.Object, _mockCollectionCalendarService.Object);

            // act
            var events = _sut.FlushEvents();

            // assert
            var expectedEvent = events.Single() as EarningsCalculated;

            expectedEvent.ApprenticeshipIncentiveId.Should().Be(_sutModel.Id);
            expectedEvent.AccountId.Should().Be(_sutModel.Account.Id);
            expectedEvent.ApprenticeshipId.Should().Be(_sutModel.Apprenticeship.Id);
            expectedEvent.ApplicationApprenticeshipId.Should().Be(_sutModel.ApplicationApprenticeshipId);
        }

        [Test]
        public async Task Then_RefreshedLearnerForEarnings_is_set_to_false_when_earnings_are_calculated()
        {
            // arrange
            _sutModel.RefreshedLearnerForEarnings = true;

            // act
            await _sut.CalculateEarnings(_mockPaymentProfilesService.Object, _mockCollectionCalendarService.Object);

            // assert
            _sut.RefreshedLearnerForEarnings.Should().BeFalse();
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
