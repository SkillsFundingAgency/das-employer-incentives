using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
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
        private Mock<IDateTimeService> _mockDateTimeService;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockDateTimeService = new Mock<IDateTimeService>();
            _mockDateTimeService.Setup(m => m.Now()).Returns(DateTime.Now);
            _mockDateTimeService.Setup(m => m.UtcNow()).Returns(DateTime.UtcNow);

            var collectionPeriods = new List<CollectionCalendarPeriod>()
            {
                new CollectionCalendarPeriod(new CollectionPeriod(1, _fixture.Create<short>()), _fixture.Create<byte>(), _fixture.Create<short>(), _fixture.Create<DateTime>(), _fixture.Create<DateTime>(), true, false),
            };

            _collectionCalendar = new CollectionCalendar(new List<AcademicYear> { new AcademicYear("2021", new DateTime(2021, 7, 31)) }, collectionPeriods);
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
                BreakInLearning.Create(new DateTime(2021, 4, 1), new DateTime(2021, 6, 9)),
                BreakInLearning.Create(new DateTime(2021, 8, 26), new DateTime(2021, 10, 1)),
            };

            var periods = new List<LearningPeriod>
            {
                new LearningPeriod(new DateTime(2021, 1, 3), new DateTime(2021, 3, 31)),
                new LearningPeriod(new DateTime(2021, 6, 10), new DateTime(2021, 8, 25)),
                new LearningPeriod(new DateTime(2021, 10, 2), new DateTime(2021, 12, 31)),
            };
            _learner.SetLearningPeriods(periods);

            // Act
            _sut.SetBreaksInLearning(_learner.LearningPeriods.ToList(), _collectionCalendar, _mockDateTimeService.Object);

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
                new LearningPeriod(new DateTime(2021, 10, 2), new DateTime(2021, 12, 31)),
            };
            _learner.SetLearningPeriods(periods);

            // Act
            _sut.SetBreaksInLearning(_learner.LearningPeriods.ToList(), _collectionCalendar, _mockDateTimeService.Object);

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
                BreakInLearning.Create(new DateTime(2021, 8, 26), new DateTime(2021, 10, 1)),
                BreakInLearning.Create(new DateTime(2021, 4, 1), new DateTime(2021, 6, 9)),
            };
            _sut.GetModel().BreakInLearnings = expected;

            var periods = new List<LearningPeriod>
            {
                new LearningPeriod(new DateTime(2021, 1, 3), new DateTime(2021, 3, 31)),
                new LearningPeriod(new DateTime(2021, 6, 10), new DateTime(2021, 8, 25)),
                new LearningPeriod(new DateTime(2021, 10, 2), new DateTime(2021, 12, 31)),
            };
            _learner.SetLearningPeriods(periods);

            // Act
            _sut.SetBreaksInLearning(_learner.LearningPeriods.ToList(), _collectionCalendar, _mockDateTimeService.Object);

            // Assert
            _sut.FlushEvents().Any(e => e is EarningsCalculated).Should().BeFalse();
        }
    }
}
