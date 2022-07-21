using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests
{
    [TestFixture]
    public class WhenChangeOfCircumstance
    {
        private ApprenticeshipIncentive _sut;
        private ApprenticeshipIncentiveModel _sutModel;
        private Fixture _fixture;
        private Learner _learner;
        private CollectionCalendar _collectionCalendar;
        private CollectionPeriod _collectionPeriod;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _collectionPeriod = new CollectionPeriod(6, 2021);

            var collectionPeriods = new List<CollectionCalendarPeriod>()
            {
                new CollectionCalendarPeriod(
                    _collectionPeriod,
                    _fixture.Create<byte>(), 
                    _fixture.Create<short>(), 
                    _fixture.Create<DateTime>(),
                    new DateTime(2022, 1, 1),
                    true, 
                    false),
            };

            _collectionCalendar = new CollectionCalendar(new List<AcademicYear>(), collectionPeriods);

            _sutModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(x => x.Status, IncentiveStatus.Active)
                .With(x => x.PreviousStatus, IncentiveStatus.Active)
                .With(x => x.Phase, new IncentivePhase(Phase.Phase1))
                .With(x => x.StartDate, new DateTime(2020, 10, 1))
                .Create();

            _sutModel.PendingPaymentModels = new List<PendingPaymentModel>
            {
                _fixture.Build<PendingPaymentModel>()
                .With(x => x.DueDate, _sutModel.StartDate.AddDays(89))
                .With(x => x.ClawedBack, false)
                .With(x => x.PaymentMadeDate, (DateTime?)null)
                .With(x => x.Amount, 1000)
                .With(x => x.EarningType, EarningType.FirstPayment)
                .With(x => x.CollectionPeriod, _collectionPeriod)
                .Create(),

                _fixture.Build<PendingPaymentModel>()
                .With(x => x.DueDate, _sutModel.StartDate.AddDays(364))
                .With(x => x.ClawedBack, false)
                .With(x => x.PaymentMadeDate, (DateTime?)null)
                .With(x => x.Amount, 1000)
                .With(x => x.EarningType, EarningType.SecondPayment)
                .With(x => x.CollectionPeriod, _collectionPeriod)
                .Create()
            };

            _sutModel.PaymentModels = new List<PaymentModel>();
            _sutModel.ClawbackPaymentModels = new List<ClawbackPaymentModel>();
            _sut = Sut(_sutModel);

            var learningData = new LearningData(true);
            learningData.SetIsStopped(new LearningStoppedStatus(true, _sutModel.PendingPaymentModels.Last().DueDate.AddDays(-1)));
            var submissionData = new SubmissionData();
            submissionData.SetSubmissionDate(DateTime.Now);
            submissionData.SetLearningData(learningData);
            _learner = Learner.New(_fixture.Create<Guid>(), _sutModel.Id, _fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<long>());
            _learner.SetSubmissionData(submissionData);
        }

        [Test]
        public void Then_unpaid_earnings_after_stop_date_are_removed()
        {
            //Arrange
            var expectedPaymentHashCode = _sutModel.PendingPaymentModels.First().GetHashCode();

            //Act
            _sut.SetLearningStoppedChangeOfCircumstance(_learner, _collectionCalendar);

            //Assert
            _sutModel.PendingPaymentModels.Count.Should().Be(1);
            _sutModel.PendingPaymentModels.First().GetHashCode().Should().Be(expectedPaymentHashCode);
        }

        [Test]
        public void Then_paid_earnings_after_stop_date_that_have_not_been_sent_are_removed()
        {
            var collectionYear = (short)2021;
            var collectionPeriod = (byte)6;
            //Arrange
            var pendingPayment = _sutModel.PendingPaymentModels.First();
            pendingPayment.PendingPaymentValidationResultModels = new List<PendingPaymentValidationResultModel>
            {
                _fixture.Build<PendingPaymentValidationResultModel>().With(x => x.CollectionPeriod, new CollectionPeriod(collectionPeriod, collectionYear)).With(x => x.ValidationResult, true).Create()
            };
            
            _sut.CreatePayment(pendingPayment.Id, new CollectionPeriod(collectionPeriod, collectionYear));
            
            pendingPayment = _sutModel.PendingPaymentModels.Last();
            pendingPayment.PendingPaymentValidationResultModels = new List<PendingPaymentValidationResultModel>
            {
                _fixture.Build<PendingPaymentValidationResultModel>().With(x => x.CollectionPeriod, new CollectionPeriod(collectionPeriod, collectionYear)).With(x => x.ValidationResult, true).Create()
            };            
            _sut.CreatePayment(pendingPayment.Id, new CollectionPeriod(collectionPeriod, collectionYear));
            
            var expectedPaymentHashCode = _sutModel.PendingPaymentModels.First().GetHashCode();

            //Act
            _sut.SetLearningStoppedChangeOfCircumstance(_learner, _collectionCalendar);

            //Assert
            _sutModel.PendingPaymentModels.Count.Should().Be(1);            
        }

        [Test]
        public void Then_paid_earnings_after_stop_date_are_clawed_back()
        {
            var collectionYear = (short)2021;
            var collectionPeriod = (byte)6;
            //Arrange
            var pendingPayment = _sutModel.PendingPaymentModels.First();
            pendingPayment.PendingPaymentValidationResultModels = new List<PendingPaymentValidationResultModel>
            {
                _fixture.Build<PendingPaymentValidationResultModel>().With(x => x.CollectionPeriod, new CollectionPeriod(collectionPeriod, collectionYear)).With(x => x.ValidationResult, true).Create()
            };

            _sut.CreatePayment(pendingPayment.Id, new CollectionPeriod(collectionPeriod, collectionYear));
            
            pendingPayment = _sutModel.PendingPaymentModels.Last();
            pendingPayment.PendingPaymentValidationResultModels = new List<PendingPaymentValidationResultModel>
            {
                _fixture.Build<PendingPaymentValidationResultModel>().With(x => x.CollectionPeriod, new CollectionPeriod(collectionPeriod, collectionYear)).With(x => x.ValidationResult, true).Create()
            };
            _sut.CreatePayment(pendingPayment.Id, new CollectionPeriod(collectionPeriod, collectionYear));

            _sutModel.PaymentModels.First().PaidDate = DateTime.Now;
            _sutModel.PaymentModels.Last().PaidDate = DateTime.Now;

            //Act
            _sut.SetLearningStoppedChangeOfCircumstance(_learner, _collectionCalendar);

            //Assert
            _sutModel.PendingPaymentModels.Count.Should().Be(2);
            _sutModel.PendingPaymentModels.First().ClawedBack.Should().BeFalse();
            _sutModel.PendingPaymentModels.Last().ClawedBack.Should().BeTrue();
            _sutModel.ClawbackPaymentModels.Count.Should().Be(1);
        }

        [TestCase(Phase.Phase1, 3, "2020-08-01", 4)]
        [TestCase(Phase.Phase1, 3, "2021-01-31", 4)]
        [TestCase(Phase.Phase1, 4, "2021-02-01", 5)]
        [TestCase(Phase.Phase1, 4, "2021-05-01", 5)]
        [TestCase(Phase.Phase1, 5, "2020-08-01", 5)]
        [TestCase(Phase.Phase1, 5, "2021-01-31", 5)]
        [TestCase(Phase.Phase1, 5, "2021-06-01", 5)]
        [TestCase(Phase.Phase2, 5, "2021-06-01", 6)]
        [TestCase(Phase.Phase2, 5, "2021-09-30", 6)]
        [TestCase(Phase.Phase3, 6, "2021-10-01", 7)]
        [TestCase(Phase.Phase3, 6, "2022-01-31", 7)]
        [TestCase(Phase.Phase3, 7, "2021-10-01", 7)]
        [TestCase(Phase.Phase3, 7, "2022-01-31", 7)]
        public void Then_minimum_agreement_version_is_set_when_the_start_date_changes(Phase phase, int agreementVersion, DateTime startDate, int expectedMinimumVersion)
        {
            // Arrange
            _sutModel.MinimumAgreementVersion = new AgreementVersion(agreementVersion);
            _sutModel.Phase = new IncentivePhase(phase);
            _sut = Sut(_sutModel);

            //Act
            _sut.SetStartDateChangeOfCircumstance(startDate, _collectionCalendar);

            //Assert
            _sut.MinimumAgreementVersion.MinimumRequiredVersion.Should().Be(expectedMinimumVersion);
        }

        [Test]
        public void Then_the_first_earning_is_calculated_if_the_updated_learning_stopped_date_is_after_earnings_due_for_payment_and_before_second_payment_due_date()
        {
            // Arrange
            var learningData = new LearningData(true);
            learningData.SetIsStopped(new LearningStoppedStatus(true, _sutModel.StartDate.AddDays(90)));
            var submissionData = new SubmissionData();
            submissionData.SetSubmissionDate(DateTime.Now);
            submissionData.SetLearningData(learningData);
            _learner = Learner.New(_fixture.Create<Guid>(), _sutModel.Id, _fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<long>());
            _learner.SetSubmissionData(submissionData);

            _sut.SetLearningStoppedChangeOfCircumstance(_learner, _collectionCalendar);

            learningData = new LearningData(true);
            learningData.SetIsStopped(new LearningStoppedStatus(true, _sutModel.StartDate.AddDays(90)));
            submissionData.SetLearningData(learningData);

            // Act
            _sut.SetLearningStoppedChangeOfCircumstance(_learner, _collectionCalendar);

            // Assert
            _sutModel.PendingPaymentModels.Count.Should().Be(1);
        }

        [Test]
        public void Then_the_first_earning_is_calculated_if_the_updated_learning_stopped_date_is_on_the_payment_due_date()
        {
            // Arrange
            var learningData = new LearningData(true);
            learningData.SetIsStopped(new LearningStoppedStatus(true, _sutModel.StartDate.AddDays(70)));
            var submissionData = new SubmissionData();
            submissionData.SetSubmissionDate(DateTime.Now);
            submissionData.SetLearningData(learningData);
            _learner = Learner.New(_fixture.Create<Guid>(), _sutModel.Id, _fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<long>());
            _learner.SetSubmissionData(submissionData);

            _sut.SetLearningStoppedChangeOfCircumstance(_learner, _collectionCalendar);

            learningData = new LearningData(true);
            learningData.SetIsStopped(new LearningStoppedStatus(true, _sutModel.StartDate.AddDays(89)));
            submissionData.SetLearningData(learningData);

            // Act
            _sut.SetLearningStoppedChangeOfCircumstance(_learner, _collectionCalendar);

            // Assert
            _sutModel.PendingPaymentModels.Count.Should().Be(1);
        }

        [Test]
        public void Then_the_second_earning_is_recalculated_if_the_first_earning_is_paid_and_the_updated_learning_stopped_date_is_after_the_due_date()
        {
            // Arrange
            Then_paid_earnings_after_stop_date_that_have_not_been_sent_are_removed();

            var learningData = new LearningData(true);
            learningData.SetIsStopped(new LearningStoppedStatus(true, _sutModel.StartDate.AddDays(365)));
            var submissionData = new SubmissionData();
            submissionData.SetSubmissionDate(DateTime.Now);
            submissionData.SetLearningData(learningData);
            _learner = Learner.New(_fixture.Create<Guid>(), _sutModel.Id, _fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<long>());
            _learner.SetSubmissionData(submissionData);

            // Act
            _sut.SetLearningStoppedChangeOfCircumstance(_learner, _collectionCalendar);

            // Assert
            _sutModel.PendingPaymentModels.Count.Should().Be(2);
        }

        [Test]
        public void Then_the_due_date_of_the_new_earning_is_not_after_the_learning_stopped_date()
        {
            // Arrange
            var learningData = new LearningData(true);
            learningData.SetIsStopped(new LearningStoppedStatus(true, _sutModel.StartDate.AddDays(88)));
            var submissionData = new SubmissionData();
            submissionData.SetSubmissionDate(DateTime.Now);
            submissionData.SetLearningData(learningData);
            _learner = Learner.New(_fixture.Create<Guid>(), _sutModel.Id, _fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<long>());
            _learner.SetSubmissionData(submissionData);

            _sut.SetLearningStoppedChangeOfCircumstance(_learner, _collectionCalendar);

            learningData = new LearningData(true);
            learningData.SetIsStopped(new LearningStoppedStatus(true, _sutModel.StartDate.AddDays(90)));
            submissionData.SetLearningData(learningData);

            // Act
            _sut.SetLearningStoppedChangeOfCircumstance(_learner, _collectionCalendar);

            // Assert
            _sutModel.PendingPaymentModels.First().DueDate.Year.Should().Be(_learner.SubmissionData.LearningData.StoppedStatus.DateStopped.Value.Year);
            _sutModel.PendingPaymentModels.First().DueDate.Month.Should().Be(_learner.SubmissionData.LearningData.StoppedStatus.DateStopped.Value.Month);
        }

        [Test]
        public void Then_the_first_payment_is_not_recalculated_if_the_next_stopped_date_received_is_before_the_due_date()
        {
            // Arrange
            var learningData = new LearningData(true);
            learningData.SetIsStopped(new LearningStoppedStatus(true, _sutModel.StartDate.AddDays(87)));
            var submissionData = new SubmissionData();
            submissionData.SetSubmissionDate(DateTime.Now);
            submissionData.SetLearningData(learningData);
            _learner = Learner.New(_fixture.Create<Guid>(), _sutModel.Id, _fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<long>());
            _learner.SetSubmissionData(submissionData);

            _sut.SetLearningStoppedChangeOfCircumstance(_learner, _collectionCalendar);

            learningData = new LearningData(true);
            learningData.SetIsStopped(new LearningStoppedStatus(true, _sutModel.StartDate.AddDays(88)));
            submissionData.SetLearningData(learningData);

            // Act
            _sut.SetLearningStoppedChangeOfCircumstance(_learner, _collectionCalendar);

            // Assert
            _sutModel.PendingPaymentModels.Count.Should().Be(0);
        }

        [Test]
        public void Then_the_second_payment_is_not_recalculated_if_the_next_stopped_date_received_is_before_the_due_date()
        {
            // Arrange
            var learningData = new LearningData(true);
            learningData.SetIsStopped(new LearningStoppedStatus(true, _sutModel.StartDate.AddDays(200)));
            var submissionData = new SubmissionData();
            submissionData.SetSubmissionDate(DateTime.Now);
            submissionData.SetLearningData(learningData);
            _learner = Learner.New(_fixture.Create<Guid>(), _sutModel.Id, _fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<long>());
            _learner.SetSubmissionData(submissionData);

            _sut.SetLearningStoppedChangeOfCircumstance(_learner, _collectionCalendar);

            learningData = new LearningData(true);
            learningData.SetIsStopped(new LearningStoppedStatus(true, _sutModel.StartDate.AddDays(363)));
            submissionData.SetLearningData(learningData);

            // Act
            _sut.SetLearningStoppedChangeOfCircumstance(_learner, _collectionCalendar);

            // Assert
            _sutModel.PendingPaymentModels.Count.Should().Be(1);
        }

        [Test]
        public void Then_the_first_payment_is_recalculated_if_the_next_stopped_date_received_is_after_the_due_date_and_the_previous_payments_were_removed()
        {
            // Arrange
            var learningData = new LearningData(true);
            learningData.SetIsStopped(new LearningStoppedStatus(true, _sutModel.StartDate.AddDays(60)));
            var submissionData = new SubmissionData();
            submissionData.SetSubmissionDate(DateTime.Now);
            submissionData.SetLearningData(learningData);
            _learner = Learner.New(_fixture.Create<Guid>(), _sutModel.Id, _fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<long>());
            _learner.SetSubmissionData(submissionData);

            _sut.SetLearningStoppedChangeOfCircumstance(_learner, _collectionCalendar);

            _sutModel.PendingPaymentModels.Count.Should().Be(0);

            learningData = new LearningData(true);
            learningData.SetIsStopped(new LearningStoppedStatus(true, _sutModel.StartDate.AddDays(104)));
            submissionData.SetLearningData(learningData);

            // Act
            _sut.SetLearningStoppedChangeOfCircumstance(_learner, _collectionCalendar);

            // Assert
            _sutModel.PendingPaymentModels.Count.Should().Be(1);
        }

        [Test]
        public void Then_the_first_payment_is_recalculated_if_the_next_stopped_date_received_is_after_the_due_date_and_the_payment_was_clawed_back()
        {
            // Arrange
            _sutModel.Status = IncentiveStatus.Stopped;
            _sutModel.PendingPaymentModels = new List<PendingPaymentModel>();
            _sutModel.PendingPaymentModels.Add(_fixture.Build<PendingPaymentModel>()
                .With(x => x.EarningType, EarningType.FirstPayment)
                .With(x => x.DueDate, new DateTime(2021, 1, 1))
                .With(x => x.ClawedBack, true)
                .With(x => x.PaymentMadeDate, _fixture.Create<DateTime>())
                .Create());
            _sutModel.PaymentModels = new List<PaymentModel>();
            _sutModel.ClawbackPaymentModels = new List<ClawbackPaymentModel> 
            {
                _fixture.Build<ClawbackPaymentModel>()
                    .With(x => x.DateClawbackSent, _fixture.Create<DateTime>())
                    .With(x => x.PendingPaymentId, _sutModel.PendingPaymentModels.ToList()[0].Id)
                    .With(x => x.ApprenticeshipIncentiveId, _sutModel.PendingPaymentModels.ToList()[0].ApprenticeshipIncentiveId)
                    .Create()
            };
            _sut = Sut(_sutModel);

            var learningData = new LearningData(true);
            learningData.SetIsStopped(new LearningStoppedStatus(true, _sutModel.StartDate.AddDays(104)));
            var submissionData = new SubmissionData();
            submissionData.SetSubmissionDate(DateTime.Now);
            submissionData.SetLearningData(learningData);
            _learner = Learner.New(_fixture.Create<Guid>(), _sutModel.Id, _fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<long>());
            _learner.SetSubmissionData(submissionData);

            // Act
            _sut.SetLearningStoppedChangeOfCircumstance(_learner, _collectionCalendar);

            // Assert
            _sutModel.PendingPaymentModels.Count.Should().Be(2);
            _sutModel.PendingPaymentModels.Count(x => x.EarningType == EarningType.FirstPayment).Should().Be(2);
            _sutModel.PendingPaymentModels.Count(x => x.EarningType == EarningType.FirstPayment && x.ClawedBack).Should().Be(1);
            _sutModel.PendingPaymentModels.Count(x => x.EarningType == EarningType.FirstPayment && !x.ClawedBack).Should().Be(1);
            _sutModel.PendingPaymentModels.Count(x => x.EarningType == EarningType.FirstPayment && !x.PaymentMadeDate.HasValue).Should().Be(1);
            _sutModel.PendingPaymentModels.Count(x => x.EarningType == EarningType.FirstPayment && x.PaymentMadeDate.HasValue).Should().Be(1);
        }

        [Test]
        public void Then_the_second_payment_is_recalculated_if_the_next_stopped_date_received_is_after_the_due_date_and_the_payment_was_clawed_back()
        {
            // Arrange
            _sutModel.Status = IncentiveStatus.Stopped;
            _sutModel.PendingPaymentModels = new List<PendingPaymentModel>();
            _sutModel.PendingPaymentModels.Add(_fixture.Build<PendingPaymentModel>()
                .With(x => x.EarningType, EarningType.FirstPayment)
                .With(x => x.DueDate, new DateTime(2021, 1, 1))
                .With(x => x.ClawedBack, false)
                .With(x => x.PaymentMadeDate, _fixture.Create<DateTime>())
                .With(x => x.Amount, 1000m)
                .Create());
            _sutModel.PendingPaymentModels.Add(_fixture.Build<PendingPaymentModel>()
                .With(x => x.EarningType, EarningType.SecondPayment)
                .With(x => x.DueDate, new DateTime(2021, 1, 1))
                .With(x => x.ClawedBack, true)
                .With(x => x.PaymentMadeDate, _fixture.Create<DateTime>())
                .With(x => x.Amount, 1000m)
                .Create());
            _sutModel.PaymentModels = new List<PaymentModel> 
            { 
                _fixture.Build<PaymentModel>()
                    .With(x => x.PaidDate, _fixture.Create<DateTime>())
                    .With(x => x.PendingPaymentId, _sutModel.PendingPaymentModels.ToList()[0].Id)
                    .With(x => x.ApprenticeshipIncentiveId, _sutModel.PendingPaymentModels.ToList()[0].ApprenticeshipIncentiveId)
                    .With(x => x.Amount, _sutModel.PendingPaymentModels.ToList()[0].Amount)
                    .Create()
            };
            _sutModel.ClawbackPaymentModels = new List<ClawbackPaymentModel>
            {
                _fixture.Build<ClawbackPaymentModel>()
                    .With(x => x.DateClawbackSent, _fixture.Create<DateTime>())
                    .With(x => x.PendingPaymentId, _sutModel.PendingPaymentModels.ToList()[1].Id)
                    .With(x => x.ApprenticeshipIncentiveId, _sutModel.PendingPaymentModels.ToList()[1].ApprenticeshipIncentiveId)
                    .With(x => x.Amount, _sutModel.PendingPaymentModels.ToList()[1].Amount)
                    .Create()
            };
            _sut = Sut(_sutModel);

            var learningData = new LearningData(true);
            learningData.SetIsStopped(new LearningStoppedStatus(true, _sutModel.StartDate.AddDays(366)));
            var submissionData = new SubmissionData();
            submissionData.SetSubmissionDate(DateTime.Now);
            submissionData.SetLearningData(learningData);
            _learner = Learner.New(_fixture.Create<Guid>(), _sutModel.Id, _fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<long>());
            _learner.SetSubmissionData(submissionData);

            // Act
            _sut.SetLearningStoppedChangeOfCircumstance(_learner, _collectionCalendar);

            // Assert
            _sutModel.PendingPaymentModels.Count.Should().Be(3);
            _sutModel.PendingPaymentModels.Count(x => x.EarningType == EarningType.SecondPayment).Should().Be(2);
            _sutModel.PendingPaymentModels.Count(x => x.EarningType == EarningType.SecondPayment && x.ClawedBack).Should().Be(1);
            _sutModel.PendingPaymentModels.Count(x => x.EarningType == EarningType.SecondPayment && !x.ClawedBack).Should().Be(1);
            _sutModel.PendingPaymentModels.Count(x => x.EarningType == EarningType.SecondPayment && !x.PaymentMadeDate.HasValue).Should().Be(1);
            _sutModel.PendingPaymentModels.Count(x => x.EarningType == EarningType.SecondPayment && x.PaymentMadeDate.HasValue).Should().Be(1);
        }

        private ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentive.Get(model.Id, model);
        }
    }
}
