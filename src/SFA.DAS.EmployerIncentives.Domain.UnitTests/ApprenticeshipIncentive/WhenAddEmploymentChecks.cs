using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.Services;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests
{
    [TestFixture]
    public class WhenAddEmploymentChecks
    {
        private ApprenticeshipIncentives.ApprenticeshipIncentive _sut;
        private ApprenticeshipIncentiveModel _sutModel;
        private Fixture _fixture;
        private IDateTimeService _dateTimeService;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _dateTimeService = new DateTimeService();
           
            _sutModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(x => x.Status, IncentiveStatus.Active)
                .With(x => x.Phase, new IncentivePhase(Phase.Phase1))
                .Without(x => x.EmploymentCheckModels)
                .Without(x => x.PendingPaymentModels)
                .Create();

            _sut = Sut(_sutModel);
        }

        [TestCase(Phase.Phase1, "2020-8-1")]
        [TestCase(Phase.Phase2, "2021-4-1")]
        [TestCase(Phase.Phase3, "2021-10-1")]
        public void Then_EmploymentBeforeSchemeCheck_is_added_when_it_does_not_already_exist_and_6_weeks_from_start_date_has_elapsed(Phase phase, DateTime eligibilityStartDate)
        {
            // arrange
            _sutModel.StartDate = _dateTimeService.UtcNow().AddDays(-42);
            _sutModel.Phase = new IncentivePhase(phase);
            _sut = Sut(_sutModel);

            // act
            _sut.AddEmploymentChecks(_dateTimeService);

            // assert
            var check = _sut.EmploymentChecks.SingleOrDefault(c => c.CheckType == EmploymentCheckType.EmployedBeforeSchemeStarted);
            check.Should().NotBeNull();
            check.MinimumDate.Should().Be(eligibilityStartDate.AddMonths(-6));
            check.MaximumDate.Should().Be(eligibilityStartDate.AddDays(-1));
            check.Result.Should().BeNull();            
        }

        [TestCase(Phase.Phase1)]
        [TestCase(Phase.Phase2)]
        [TestCase(Phase.Phase3)]
        public void Then_a_new_EmploymentChecksCreated_event_is_added_when_the_EmploymentBeforeSchemeCheck_is_added(Phase phase)
        {
            _sutModel.StartDate = _dateTimeService.UtcNow().AddDays(-42);
            _sutModel.Phase = new IncentivePhase(phase);
            _sut = Sut(_sutModel);

            // act
            _sut.AddEmploymentChecks(_dateTimeService);

            // assert
            var events = _sut.FlushEvents().OfType<EmploymentChecksCreated>();
            events.Count().Should().Be(2);
            @events.First().ApprenticeshipIncentiveId.Should().Be(_sut.Id);
            @events.First().Model.CheckType.Should().Be(EmploymentCheckType.EmployedBeforeSchemeStarted);
            @events.Last().ApprenticeshipIncentiveId.Should().Be(_sut.Id);
            @events.Last().Model.CheckType.Should().Be(EmploymentCheckType.EmployedAtStartOfApprenticeship);
        }

        [Test]
        public void Then_EmployedAtStartOfApprenticeshipCheck_is_added_when_it_does_not_already_exist_and_6_weeks_from_start_date_has_elapsed()
        {
            // arrange
            _sutModel.StartDate = _dateTimeService.UtcNow().AddDays(-42);
            _sut = Sut(_sutModel);

            // act
            _sut.AddEmploymentChecks(_dateTimeService);

            // assert
            var check = _sut.EmploymentChecks.SingleOrDefault(c => c.CheckType == EmploymentCheckType.EmployedAtStartOfApprenticeship);
            check.Should().NotBeNull();
            check.MinimumDate.Should().Be(_sutModel.StartDate);
            check.MaximumDate.Should().Be(_sutModel.StartDate.AddDays(42));
            check.Result.Should().BeNull();
        }

        [Test]
        public void Then_a_new_EmploymentChecksCreated_event_is_added_when_the_EmployedAtStartOfApprenticeshipCheck_is_added()
        {
            _sutModel.StartDate = _dateTimeService.UtcNow().AddDays(-42);
            _sut = Sut(_sutModel);

            // act
            _sut.AddEmploymentChecks(_dateTimeService);

            // assert
            var events = _sut.FlushEvents().OfType<EmploymentChecksCreated>();
            events.Count().Should().Be(2);
            @events.First().ApprenticeshipIncentiveId.Should().Be(_sut.Id);
            @events.First().Model.CheckType.Should().Be(EmploymentCheckType.EmployedBeforeSchemeStarted);
            @events.Last().ApprenticeshipIncentiveId.Should().Be(_sut.Id);
            @events.Last().Model.CheckType.Should().Be(EmploymentCheckType.EmployedAtStartOfApprenticeship);
        }

        [Test]
        public void Then_EmployedAt365Check_is_added_when_it_does_not_already_exist_and_and_3_weeks_after_second_payment_due_date_has_elapsed_and_previous_checks_have_passed()
        {
            var paymentDueDate = _dateTimeService.UtcNow().AddDays(-21);

            _sutModel.EmploymentCheckModels = new List<EmploymentCheckModel>()
            {
                _fixture.Build<EmploymentCheckModel>()
                .With(c => c.CheckType, EmploymentCheckType.EmployedAtStartOfApprenticeship)
                .With(c => c.Result, true)
                .Create(),
                _fixture.Build<EmploymentCheckModel>()
                .With(c => c.CheckType, EmploymentCheckType.EmployedBeforeSchemeStarted)
                .With(c => c.Result, false
                )
                .Create()
            };

            _sutModel.PendingPaymentModels.Add(
                _fixture.Build<PendingPaymentModel>()
                .With(pp => pp.ApprenticeshipIncentiveId, _sut.Id)
                .With(pp => pp.EarningType, EarningType.SecondPayment)
                .With(pp => pp.DueDate, paymentDueDate)
                .Without(pp => pp.PaymentMadeDate)
                .Create());

            _sut = Sut(_sutModel);

            // act
            _sut.AddEmploymentChecks(_dateTimeService);

            // assert
            var check = _sut.EmploymentChecks.SingleOrDefault(c => c.CheckType == EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck);
            check.Should().NotBeNull();
            check.MinimumDate.Should().Be(paymentDueDate);
            check.MaximumDate.Should().Be(paymentDueDate.AddDays(21));
            check.Result.Should().BeNull();
        }

        [Test]
        public void Then_a_new_EmploymentChecksCreated_event_is_added_when_the_EmployedAt365Check_is_added()
        {
            var paymentDueDate = _dateTimeService.UtcNow().AddDays(-21);

            _sutModel.EmploymentCheckModels = new List<EmploymentCheckModel>()
            {
                _fixture.Build<EmploymentCheckModel>()
                .With(c => c.CheckType, EmploymentCheckType.EmployedAtStartOfApprenticeship)
                .With(c => c.Result, true)
                .Create(),
                _fixture.Build<EmploymentCheckModel>()
                .With(c => c.CheckType, EmploymentCheckType.EmployedBeforeSchemeStarted)
                .With(c => c.Result, false
                )
                .Create()
            };

            _sutModel.PendingPaymentModels.Add(
                _fixture.Build<PendingPaymentModel>()
                .With(pp => pp.ApprenticeshipIncentiveId, _sut.Id)
                .With(pp => pp.EarningType, EarningType.SecondPayment)
                .With(pp => pp.DueDate, paymentDueDate)
                .Without(pp => pp.PaymentMadeDate)
                .Create());

            _sut = Sut(_sutModel);

            // act
            _sut.AddEmploymentChecks(_dateTimeService);

            // assert
            var events = _sut.FlushEvents().ToList();
            var @event = events.Single() as EmploymentChecksCreated;
            @event.ApprenticeshipIncentiveId.Should().Be(_sut.Id);
        }

        [TestCase(Phase.Phase1)]
        [TestCase(Phase.Phase2)]
        [TestCase(Phase.Phase3)]
        public void Then_EmploymentBeforeSchemeCheck_is_not_added_when_the_incentive_is_withdrawn(Phase phase)
        {
            // arrange
            _sutModel.StartDate = _dateTimeService.UtcNow().AddDays(-42);
            _sutModel.Phase = new IncentivePhase(phase);
            _sutModel.Status = IncentiveStatus.Withdrawn;
            _sut = Sut(_sutModel);

            // act
            _sut.AddEmploymentChecks(_dateTimeService);

            // assert
            _sut.EmploymentChecks.Any(c => c.CheckType == EmploymentCheckType.EmployedBeforeSchemeStarted).Should().BeFalse();
        }

        [Test]
        public void Then_EmployedAtStartOfApprenticeshipCheck_is_not_added_when_the_incentive_is_withdrawn()
        {
            // arrange
            _sutModel.StartDate = _dateTimeService.UtcNow().AddDays(-42);
            _sutModel.Status = IncentiveStatus.Withdrawn;
            _sut = Sut(_sutModel);

            // act
            _sut.AddEmploymentChecks(_dateTimeService);

            // assert
            _sut.EmploymentChecks.Any(c => c.CheckType == EmploymentCheckType.EmployedAtStartOfApprenticeship).Should().BeFalse();
        }

        [Test]
        public void Then_EmployedAt365Check_is_not_added_when_the_incentive_is_withdrawn()
        {
            var paymentDueDate = _dateTimeService.UtcNow().AddDays(-21);

            _sutModel.Status = IncentiveStatus.Withdrawn;
            _sutModel.EmploymentCheckModels = new List<EmploymentCheckModel>()
            {
                _fixture.Build<EmploymentCheckModel>()
                .With(c => c.CheckType, EmploymentCheckType.EmployedAtStartOfApprenticeship)
                .With(c => c.Result, true)
                .Create(),
                _fixture.Build<EmploymentCheckModel>()
                .With(c => c.CheckType, EmploymentCheckType.EmployedBeforeSchemeStarted)
                .With(c => c.Result, false
                )
                .Create()
            };

            _sutModel.PendingPaymentModels.Add(
                _fixture.Build<PendingPaymentModel>()
                .With(pp => pp.ApprenticeshipIncentiveId, _sut.Id)
                .With(pp => pp.EarningType, EarningType.SecondPayment)
                .With(pp => pp.DueDate, paymentDueDate)
                .Create());

            _sut = Sut(_sutModel);

            // act
            _sut.AddEmploymentChecks(_dateTimeService);

            // assert
            _sut.EmploymentChecks.Any(c => c.CheckType == EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck).Should().BeFalse();
        }

        [TestCase(Phase.Phase1)]
        [TestCase(Phase.Phase2)]
        [TestCase(Phase.Phase3)]
        public void Then_EmploymentBeforeSchemeCheck_is_not_added_when_it_does_not_already_exist__less_than_6_weeks_from_start_date_has_elapsed(Phase phase)
        {
            // arrange
            _sutModel.StartDate = _dateTimeService.UtcNow().AddDays(-41);
            _sutModel.Phase = new IncentivePhase(phase);
            _sut = Sut(_sutModel);

            // act
            _sut.AddEmploymentChecks(_dateTimeService);

            // assert
            _sut.EmploymentChecks.Any(c => c.CheckType == EmploymentCheckType.EmployedBeforeSchemeStarted).Should().BeFalse();
        }

        [Test]
        public void Then_EmployedAtStartOfApprenticeshipCheck_is_not_added_when_less_than_6_weeks_from_start_date_has_elapsed()
        {
            // arrange
            _sutModel.StartDate = _dateTimeService.UtcNow().AddDays(-41);
            _sut = Sut(_sutModel);

            // act
            _sut.AddEmploymentChecks(_dateTimeService);

            // assert
            _sut.EmploymentChecks.Any(c => c.CheckType == EmploymentCheckType.EmployedAtStartOfApprenticeship).Should().BeFalse();
        }

        [Test]
        public void Then_EmployedAt365Check_is_not_added_when_less_than_3_weeks_after_the_second_payment_is_due()
        {
            var paymentDueDate = _dateTimeService.UtcNow().AddDays(-20);

            _sutModel.EmploymentCheckModels = new List<EmploymentCheckModel>()
            {
                _fixture.Build<EmploymentCheckModel>()
                .With(c => c.CheckType, EmploymentCheckType.EmployedAtStartOfApprenticeship)
                .With(c => c.Result, true)
                .Create(),
                _fixture.Build<EmploymentCheckModel>()
                .With(c => c.CheckType, EmploymentCheckType.EmployedBeforeSchemeStarted)
                .With(c => c.Result, false
                )
                .Create()
            };

            _sutModel.PendingPaymentModels.Add(
                _fixture.Build<PendingPaymentModel>()
                .With(pp => pp.ApprenticeshipIncentiveId, _sut.Id)
                .With(pp => pp.EarningType, EarningType.SecondPayment)
                .With(pp => pp.DueDate, paymentDueDate)
                .Without(pp => pp.PaymentMadeDate)
                .Create());

            _sut = Sut(_sutModel);

            // act
            _sut.AddEmploymentChecks(_dateTimeService);

            // assert
            _sut.EmploymentChecks.Any(c => c.CheckType == EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck).Should().BeFalse();
        }

        [Test]
        public void Then_EmployedAt365Check_is_not_added_3_weeks_after_the_second_payment_is_due_and_EmploymentBeforeSchemeCheck_has_failed()
        {
            var paymentDueDate = _dateTimeService.UtcNow().AddDays(-21);

            _sutModel.EmploymentCheckModels = new List<EmploymentCheckModel>()
            {
                _fixture.Build<EmploymentCheckModel>()
                .With(c => c.CheckType, EmploymentCheckType.EmployedAtStartOfApprenticeship)
                .With(c => c.Result, true)
                .Create(),
                _fixture.Build<EmploymentCheckModel>()
                .With(c => c.CheckType, EmploymentCheckType.EmployedBeforeSchemeStarted)
                .With(c => c.Result, true
                )
                .Create()
            };

            _sutModel.PendingPaymentModels.Add(
                _fixture.Build<PendingPaymentModel>()
                .With(pp => pp.ApprenticeshipIncentiveId, _sut.Id)
                .With(pp => pp.EarningType, EarningType.SecondPayment)
                .With(pp => pp.DueDate, paymentDueDate)
                .Without(pp => pp.PaymentMadeDate)
                .Create());

            _sut = Sut(_sutModel);

            // act
            _sut.AddEmploymentChecks(_dateTimeService);

            // assert
            _sut.EmploymentChecks.Any(c => c.CheckType == EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck).Should().BeFalse();
        }

        [Test]
        public void Then_EmployedAt365Check_is_not_added_3_weeks_after_the_second_payment_is_due_and_EmployedAtStartOfApprenticeshipCheck_has_failed()
        {
            var paymentDueDate = _dateTimeService.UtcNow().AddDays(-21);

            _sutModel.EmploymentCheckModels = new List<EmploymentCheckModel>()
            {
                _fixture.Build<EmploymentCheckModel>()
                .With(c => c.CheckType, EmploymentCheckType.EmployedAtStartOfApprenticeship)
                .With(c => c.Result, false)
                .Create(),
                _fixture.Build<EmploymentCheckModel>()
                .With(c => c.CheckType, EmploymentCheckType.EmployedBeforeSchemeStarted)
                .With(c => c.Result, false
                )
                .Create()
            };

            _sutModel.PendingPaymentModels.Add(
                _fixture.Build<PendingPaymentModel>()
                .With(pp => pp.ApprenticeshipIncentiveId, _sut.Id)
                .With(pp => pp.EarningType, EarningType.SecondPayment)
                .With(pp => pp.DueDate, paymentDueDate)
                .Without(pp => pp.PaymentMadeDate)
                .Create());

            _sut = Sut(_sutModel);

            // act
            _sut.AddEmploymentChecks(_dateTimeService);

            // assert
            _sut.EmploymentChecks.Any(c => c.CheckType == EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck).Should().BeFalse();
        }

        [Test]
        public void Then_EmployedAt365Check_is_not_added_3_weeks_after_the_second_payment_is_due_and_EmployedAt365Check_already_exists_and_has_succeeded()
        {
            var paymentDueDate = _dateTimeService.UtcNow().AddDays(-21);

            _sutModel.EmploymentCheckModels = new List<EmploymentCheckModel>()
            {
                _fixture.Build<EmploymentCheckModel>()
                    .With(c => c.CheckType, EmploymentCheckType.EmployedAtStartOfApprenticeship)
                    .With(c => c.Result, true)
                .Create(),
                _fixture.Build<EmploymentCheckModel>()
                    .With(c => c.CheckType, EmploymentCheckType.EmployedBeforeSchemeStarted)
                    .With(c => c.Result, false)
                .Create(),
                _fixture.Build<EmploymentCheckModel>()
                    .With(c => c.CheckType, EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck)
                    .With(c => c.Result, true)
                .Create()
            };

            _sutModel.PendingPaymentModels.Add(
                _fixture.Build<PendingPaymentModel>()
                .With(pp => pp.ApprenticeshipIncentiveId, _sut.Id)
                .With(pp => pp.EarningType, EarningType.SecondPayment)
                .With(pp => pp.DueDate, paymentDueDate)
                .Without(pp => pp.PaymentMadeDate)
                .Create());

            _sut = Sut(_sutModel);

            // act
            _sut.AddEmploymentChecks(_dateTimeService);

            // assert
            var events = _sut.FlushEvents().ToList();
            _ = events.Any().Should().BeFalse();
        }

        [Test]
        public void Then_EmployedAt365Check_is_added_3_weeks_after_the_second_payment_is_due_and_EmployedAt365Check_already_exists_and_has_failed()
        {
            var paymentDueDate = _dateTimeService.UtcNow().AddDays(-42);

            _sutModel.EmploymentCheckModels = new List<EmploymentCheckModel>()
            {
                _fixture.Build<EmploymentCheckModel>()
                    .With(c => c.CheckType, EmploymentCheckType.EmployedAtStartOfApprenticeship)
                    .With(c => c.Result, true)
                .Create(),
                _fixture.Build<EmploymentCheckModel>()
                    .With(c => c.CheckType, EmploymentCheckType.EmployedBeforeSchemeStarted)
                    .With(c => c.Result, false)
                .Create(),
                _fixture.Build<EmploymentCheckModel>()
                    .With(c => c.CheckType, EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck)
                    .With(c => c.Result, false)
                .Create()
            };

            _sutModel.PendingPaymentModels.Add(
                _fixture.Build<PendingPaymentModel>()
                .With(pp => pp.ApprenticeshipIncentiveId, _sut.Id)
                .With(pp => pp.EarningType, EarningType.SecondPayment)
                .With(pp => pp.DueDate, paymentDueDate)
                .Without(pp => pp.PaymentMadeDate)
                .Create());

            _sut = Sut(_sutModel);

            // act
            _sut.AddEmploymentChecks(_dateTimeService);

            // assert
            var check = _sut.EmploymentChecks.SingleOrDefault(c => c.CheckType == EmploymentCheckType.EmployedAt365PaymentDueDateSecondCheck);
            check.Should().NotBeNull();
            check.MinimumDate.Should().Be(paymentDueDate);
            check.MaximumDate.Should().Be(paymentDueDate.AddDays(42));
            check.Result.Should().BeNull();

            _sut.EmploymentChecks.Any(c => c.CheckType == EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck).Should().BeTrue();
        }

        [Test]
        public void Then_a_new_EmploymentChecksCreated_event_is_added_3_weeks_after_the_second_payment_is_due_and_EmployedAt365Check_already_exists_and_has_failed()
        {
            var paymentDueDate = _dateTimeService.UtcNow().AddDays(-42);

            var failedEmploymentCheckModel = _fixture.Build<EmploymentCheckModel>()
                    .With(c => c.CheckType, EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck)
                    .With(c => c.Result, false)
                .Create();

            _sutModel.EmploymentCheckModels = new List<EmploymentCheckModel>()
            {
                _fixture.Build<EmploymentCheckModel>()
                    .With(c => c.CheckType, EmploymentCheckType.EmployedAtStartOfApprenticeship)
                    .With(c => c.Result, true)
                .Create(),
                _fixture.Build<EmploymentCheckModel>()
                    .With(c => c.CheckType, EmploymentCheckType.EmployedBeforeSchemeStarted)
                    .With(c => c.Result, false)
                .Create(),
                failedEmploymentCheckModel
            };

            _sutModel.PendingPaymentModels.Add(
                _fixture.Build<PendingPaymentModel>()
                .With(pp => pp.ApprenticeshipIncentiveId, _sut.Id)
                .With(pp => pp.EarningType, EarningType.SecondPayment)
                .With(pp => pp.DueDate, paymentDueDate)
                .Without(pp => pp.PaymentMadeDate)
                .Create());

            _sut = Sut(_sutModel);

            // act
            _sut.AddEmploymentChecks(_dateTimeService);

            // assert
            var events = _sut.FlushEvents().ToList();
            var createdEvent = events.Single(e => e is EmploymentChecksCreated) as EmploymentChecksCreated;
            createdEvent.ApprenticeshipIncentiveId.Should().Be(_sut.Id);
        }

        [Test]
        public void Then_a_new_EmploymentChecksCreated_event_is_not_created_if_the_first_365_day_check_has_not_been_triggered_and_the_second_payment_has_been_paid()
        {
            var paymentDueDate = _dateTimeService.UtcNow().AddMonths(-1).AddDays(-21);

            _sutModel.EmploymentCheckModels = new List<EmploymentCheckModel>()
            {
                _fixture.Build<EmploymentCheckModel>()
                    .With(c => c.CheckType, EmploymentCheckType.EmployedAtStartOfApprenticeship)
                    .With(c => c.Result, true)
                    .Create(),
                _fixture.Build<EmploymentCheckModel>()
                    .With(c => c.CheckType, EmploymentCheckType.EmployedBeforeSchemeStarted)
                    .With(c => c.Result, false)
                    .Create()
            };

            _sutModel.PendingPaymentModels.Add(
                _fixture.Build<PendingPaymentModel>()
                    .With(pp => pp.ApprenticeshipIncentiveId, _sut.Id)
                    .With(pp => pp.EarningType, EarningType.SecondPayment)
                    .With(pp => pp.DueDate, paymentDueDate)
                    .With(pp => pp.PaymentMadeDate, _dateTimeService.UtcNow().AddMonths(-1))
                    .Create());

            _sut = Sut(_sutModel);

            // act
            _sut.AddEmploymentChecks(_dateTimeService);

            // assert
            var events = _sut.FlushEvents().ToList();
            _ = events.Any().Should().BeFalse();
        }


        [Test]
        public void Then_a_new_EmploymentChecksCreated_event_is_not_created_if_the_first_365_day_check_has_failed_and_the_second_payment_has_been_paid()
        {
            var paymentDueDate = _dateTimeService.UtcNow().AddMonths(-1).AddDays(-21);

            _sutModel.EmploymentCheckModels = new List<EmploymentCheckModel>()
            {
                _fixture.Build<EmploymentCheckModel>()
                    .With(c => c.CheckType, EmploymentCheckType.EmployedAtStartOfApprenticeship)
                    .With(c => c.Result, true)
                    .Create(),
                _fixture.Build<EmploymentCheckModel>()
                    .With(c => c.CheckType, EmploymentCheckType.EmployedBeforeSchemeStarted)
                    .With(c => c.Result, false)
                    .Create(),
                _fixture.Build<EmploymentCheckModel>()
                    .With(c => c.CheckType, EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck)
                    .With(c => c.Result, false)
                    .Create()
            };

            _sutModel.PendingPaymentModels.Add(
                _fixture.Build<PendingPaymentModel>()
                    .With(pp => pp.ApprenticeshipIncentiveId, _sut.Id)
                    .With(pp => pp.EarningType, EarningType.SecondPayment)
                    .With(pp => pp.DueDate, paymentDueDate)
                    .With(pp => pp.PaymentMadeDate, _dateTimeService.UtcNow().AddMonths(-1))
                    .Create());

            _sut = Sut(_sutModel);

            // act
            _sut.AddEmploymentChecks(_dateTimeService);

            // assert
            var events = _sut.FlushEvents().ToList();
            _ = events.Any().Should().BeFalse();
        }

        private ApprenticeshipIncentives.ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentives.ApprenticeshipIncentive.Get(model.Id, model);
        }
    }
}
