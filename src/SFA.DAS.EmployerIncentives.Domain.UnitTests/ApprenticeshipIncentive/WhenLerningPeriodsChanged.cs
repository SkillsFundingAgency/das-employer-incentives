﻿using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
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
    public class WhenLerningPeriodsChanged
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

            _collectionCalendar = new CollectionCalendar(new List<AcademicYear>(), collectionPeriods);
            _paymentProfiles = new IncentivePaymentProfileListBuilder().Build();

            _sutModel = _fixture.Build<ApprenticeshipIncentiveModel>().With(x => x.Status, IncentiveStatus.Active).Create();
            _sutModel.PendingPaymentModels = new List<PendingPaymentModel>();
            _sutModel.PendingPaymentModels.Add(_fixture.Build<PendingPaymentModel>().With(x => x.DueDate, new DateTime(2021, 1, 1)).With(x => x.ClawedBack, false).With(x => x.PaymentMadeDate, (DateTime?)null).Create());
            _sutModel.PendingPaymentModels.Add(_fixture.Build<PendingPaymentModel>().With(x => x.DueDate, new DateTime(2021, 2, 28)).With(x => x.ClawedBack, false).With(x => x.PaymentMadeDate, (DateTime?)null).Create());
            _sutModel.PaymentModels = new List<PaymentModel>();
            _sutModel.ClawbackPaymentModels = new List<ClawbackPaymentModel>();
            _sut = Sut(_sutModel);


        }

        [Test]
        public void Then_breaks_in_learning_are_updated()
        {
            // Arrange
            IList<BreakInLearning> expected = new List<BreakInLearning>()
            {
                new BreakInLearning(new DateTime(2021, 4, 1)).SetEndDate(new DateTime(2021, 6, 9)),
                new BreakInLearning(new DateTime(2021, 8, 26)).SetEndDate(new DateTime(2021, 10, 1)),
            };

            var learningData = new LearningData(true);
            learningData.SetIsStopped(new LearningStoppedStatus(true, _sutModel.PendingPaymentModels.Last().DueDate.AddDays(-1)));
            var submissionData = new SubmissionData();
            submissionData.SetSubmissionDate(DateTime.Now);
            submissionData.SetLearningData(learningData);
            _learner = Learner.New(_fixture.Create<Guid>(), _sutModel.Id, _fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<long>());
            IEnumerable<LearningPeriod> periods = new List<LearningPeriod>
            {
                new LearningPeriod(new DateTime(2021, 1, 3), new DateTime(2021, 3, 31)),
                new LearningPeriod(new DateTime(2021, 6, 10), new DateTime(2021, 8, 25)),
                new LearningPeriod(new DateTime(2021, 10, 2)),
            };
            _learner.SetLearningPeriods(periods);

            // Act
            _sut.UpdateBreaksInLearning(_learner);

            // Assert
            _sut.BreakInLearnings.Should().BeEquivalentTo(expected);
        }

        private ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentive.Get(model.Id, model);
        }
    }
}
