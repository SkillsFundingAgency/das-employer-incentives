using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.Builders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests
{
    [TestFixture]
    public class WhenLearningPeriodsChanged
    {
        private ApprenticeshipIncentive _sut;
        private ApprenticeshipIncentiveModel _sutModel;
        private Fixture _fixture;
        private Learner _learner;
        private CollectionCalendar _collectionCalendar;
        private IEnumerable<IncentivePaymentProfile> _paymentProfiles;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var collectionPeriods = new List<CollectionCalendarPeriod>()
            {
                new CollectionCalendarPeriod(new CollectionPeriod(1, _fixture.Create<short>()), _fixture.Create<byte>(), _fixture.Create<short>(), _fixture.Create<DateTime>(), _fixture.Create<DateTime>(), true, false),
            };

            _collectionCalendar = new CollectionCalendar(new List<AcademicYear> { new AcademicYear("2021", new DateTime(2021, 7, 31)) }, collectionPeriods);
            _paymentProfiles = new IncentivePaymentProfileListBuilder().Build();

            _sutModel = _fixture.Build<ApprenticeshipIncentiveModel>().With(x => x.Status, IncentiveStatus.Active).Create();
            _sutModel.PendingPaymentModels = new List<PendingPaymentModel>
            {
                _fixture.Build<PendingPaymentModel>().With(x => x.DueDate, new DateTime(2021, 1, 1))
                    .With(x => x.ClawedBack, false).With(x => x.PaymentMadeDate, (DateTime?) null).Create(),
                _fixture.Build<PendingPaymentModel>().With(x => x.DueDate, new DateTime(2021, 2, 28))
                    .With(x => x.ClawedBack, false).With(x => x.PaymentMadeDate, (DateTime?) null).Create()
            };
            _sutModel.PaymentModels = new List<PaymentModel>();
            _sutModel.ClawbackPaymentModels = new List<ClawbackPaymentModel>();
            _sutModel.Phase = new IncentivePhase(Phase.Phase2);
            _sutModel.StartDate = new DateTime(2021, 7, 1);
            _sut = ApprenticeshipIncentive.Get(_sutModel.Id, _sutModel);

            var learningData = new LearningData(true);
            learningData.SetLearningPeriodsChanged(true);
            learningData.SetStartDate(new DateTime(2021, 7, 1));

            var submissionData = new SubmissionData();
            submissionData.SetSubmissionDate(DateTime.Now);
            submissionData.SetLearningData(learningData);
            _learner = Learner.New(_fixture.Create<Guid>(), _sutModel.Id, _fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<long>());
            _learner.SetSubmissionData(submissionData);
        }

        [Test]
        public void Then_breaks_in_learning_are_updated()
        {
            // Arrange
            var expected = new List<BreakInLearning>()
            {
                new BreakInLearning(new DateTime(2021, 4, 1)).SetEndDate(new DateTime(2021, 6, 9)),
                new BreakInLearning(new DateTime(2021, 8, 26)).SetEndDate(new DateTime(2021, 10, 1)),
            };

            var periods = new List<LearningPeriod>
            {
                new LearningPeriod(new DateTime(2021, 1, 3), new DateTime(2021, 3, 31)),
                new LearningPeriod(new DateTime(2021, 6, 10), new DateTime(2021, 8, 25)),
                new LearningPeriod(new DateTime(2021, 10, 2)),
            };
            _learner.SetLearningPeriods(periods);

            // Act
            _sut.SetBreaksInLearning(_learner.LearningPeriods.ToList(), _paymentProfiles, _collectionCalendar);

            // Assert
            _sut.BreakInLearnings.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Then_earnings_are_recalculated()
        {
            // Arrange
            var periods = new List<LearningPeriod>
            {
                new LearningPeriod(new DateTime(2021, 1, 3), new DateTime(2021, 3, 31)),
                new LearningPeriod(new DateTime(2021, 6, 10), new DateTime(2021, 8, 25)),
                new LearningPeriod(new DateTime(2021, 10, 2)),
            };
            _learner.SetLearningPeriods(periods);

            // Act
            _sut.SetBreaksInLearning(_learner.LearningPeriods.ToList(), _paymentProfiles, _collectionCalendar);

            // Assert
            var @event = _sut.FlushEvents().Single(e => e is EarningsCalculated) as EarningsCalculated;
            @event.ApprenticeshipIncentiveId.Should().Be(_sut.Id);
            @event.AccountId.Should().Be(_sut.GetModel().Account.Id);
            @event.ApprenticeshipId.Should().Be(_sut.GetModel().Apprenticeship.Id);
            @event.ApplicationApprenticeshipId.Should().Be(_sut.GetModel().ApplicationApprenticeshipId);
        }

        [Test]
        public void Then_earnings_are_not_recalculated_if_learning_periods_have_not_changed()
        {
            // Arrange
            var expected = new List<BreakInLearning>
            {
                new BreakInLearning(new DateTime(2021, 8, 26)).SetEndDate(new DateTime(2021, 10, 1)),
                new BreakInLearning(new DateTime(2021, 4, 1)).SetEndDate(new DateTime(2021, 6, 9)),
            };
            _sut.GetModel().BreakInLearnings = expected;

            var periods = new List<LearningPeriod>
            {
                new LearningPeriod(new DateTime(2021, 1, 3), new DateTime(2021, 3, 31)),
                new LearningPeriod(new DateTime(2021, 6, 10), new DateTime(2021, 8, 25)),
                new LearningPeriod(new DateTime(2021, 10, 2)),
            };
            _learner.SetLearningPeriods(periods);

            // Act
            _sut.SetBreaksInLearning(_learner.LearningPeriods.ToList(), _paymentProfiles, _collectionCalendar);

            // Assert
            _sut.FlushEvents().Any(e => e is EarningsCalculated).Should().BeFalse();
        }
    }
}
