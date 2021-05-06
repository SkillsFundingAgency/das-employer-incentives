﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests
{
    [TestFixture]
    public class WhenChangeOfCircumstance
    {
        private ApprenticeshipIncentive _sut;
        private ApprenticeshipIncentiveModel _sutModel;
        private Fixture _fixture;
        private Learner _learner;
        private Mock<ICollectionCalendarService> _mockCollectionCalendarService;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var collectionPeriods = new List<CollectionPeriod>()
            {
                new CollectionPeriod(1, _fixture.Create<byte>(), _fixture.Create<short>(), _fixture.Create<DateTime>(), _fixture.Create<DateTime>(), _fixture.Create<short>(), true),
            };
            var collectionCalendar = new CollectionCalendar(collectionPeriods);

            _mockCollectionCalendarService = new Mock<ICollectionCalendarService>();
            _mockCollectionCalendarService.Setup(m => m.Get()).ReturnsAsync(collectionCalendar);

            _sutModel = _fixture.Build<ApprenticeshipIncentiveModel>().With(x => x.Status, IncentiveStatus.Active).Create();
            _sutModel.PendingPaymentModels = new List<PendingPaymentModel>();
            _sutModel.PendingPaymentModels.Add(_fixture.Build<PendingPaymentModel>().With(x => x.DueDate, new DateTime(2021, 1, 1)).With(x => x.ClawedBack, false).With(x => x.PaymentMadeDate, (DateTime?)null).Create());
            _sutModel.PendingPaymentModels.Add(_fixture.Build<PendingPaymentModel>().With(x => x.DueDate, new DateTime(2021, 2, 28)).With(x => x.ClawedBack, false).With(x => x.PaymentMadeDate, (DateTime?)null).Create());
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
        public async Task Then_unpaid_earnings_after_stop_date_are_removed()
        {
            //Arrange
            var expectedPaymentHashCode = _sutModel.PendingPaymentModels.First().GetHashCode();

            //Act
            await _sut.SetChangeOfCircumstances(_learner, _mockCollectionCalendarService.Object);

            //Assert
            _sutModel.PendingPaymentModels.Count.Should().Be(1);
            _sutModel.PendingPaymentModels.First().GetHashCode().Should().Be(expectedPaymentHashCode);
        }

        [Test]
        public async Task Then_paid_earnings_after_stop_date_that_have_not_been_sent_are_removed()
        {
            var collectionYear = (short)2021;
            var collectionPeriod = (byte)6;
            //Arrange
            var pendingPayment = _sutModel.PendingPaymentModels.First();
            pendingPayment.PendingPaymentValidationResultModels = new List<PendingPaymentValidationResultModel>();
            pendingPayment.PendingPaymentValidationResultModels.Add(_fixture.Build<PendingPaymentValidationResultModel>().With(x => x.CollectionPeriod, new CollectionPeriod(collectionPeriod, collectionYear)).With(x => x.Result, true).Create());
            _sut.CreatePayment(pendingPayment.Id, collectionYear, collectionPeriod);

            pendingPayment = _sutModel.PendingPaymentModels.Last();
            pendingPayment.PendingPaymentValidationResultModels = new List<PendingPaymentValidationResultModel>();
            pendingPayment.PendingPaymentValidationResultModels.Add(_fixture.Build<PendingPaymentValidationResultModel>().With(x => x.CollectionPeriod, new CollectionPeriod(collectionPeriod, collectionYear)).With(x => x.Result, true).Create());
            _sut.CreatePayment(pendingPayment.Id, collectionYear, collectionPeriod);

            var expectedPaymentHashCode = _sutModel.PendingPaymentModels.First().GetHashCode();

            //Act
            await _sut.SetChangeOfCircumstances(_learner, _mockCollectionCalendarService.Object);

            //Assert
            _sutModel.PendingPaymentModels.Count.Should().Be(1);
            _sutModel.PendingPaymentModels.First().GetHashCode().Should().Be(expectedPaymentHashCode);
        }

        [Test]
        public async Task Then_paid_earnings_after_stop_date_are_clawed_back()
        {
            var collectionYear = (short)2021;
            var collectionPeriod = (byte)6;
            //Arrange
            var pendingPayment = _sutModel.PendingPaymentModels.First();
            pendingPayment.PendingPaymentValidationResultModels = new List<PendingPaymentValidationResultModel>();
            pendingPayment.PendingPaymentValidationResultModels.Add(_fixture.Build<PendingPaymentValidationResultModel>().With(x => x.CollectionPeriod, new CollectionPeriod(collectionPeriod, collectionYear)).With(x => x.Result, true).Create());
            _sut.CreatePayment(pendingPayment.Id, collectionYear, collectionPeriod);
            
            pendingPayment = _sutModel.PendingPaymentModels.Last();
            pendingPayment.PendingPaymentValidationResultModels = new List<PendingPaymentValidationResultModel>();
            pendingPayment.PendingPaymentValidationResultModels.Add(_fixture.Build<PendingPaymentValidationResultModel>().With(x => x.CollectionPeriod, new CollectionPeriod(collectionPeriod, collectionYear)).With(x => x.Result, true).Create());
            _sut.CreatePayment(pendingPayment.Id, collectionYear, collectionPeriod);

            _sutModel.PaymentModels.First().PaidDate = DateTime.Now;
            _sutModel.PaymentModels.Last().PaidDate = DateTime.Now;

            //Act
            await _sut.SetChangeOfCircumstances(_learner, _mockCollectionCalendarService.Object);

            //Assert
            _sutModel.PendingPaymentModels.Count.Should().Be(2);
            _sutModel.PendingPaymentModels.First().ClawedBack.Should().BeFalse();
            _sutModel.PendingPaymentModels.Last().ClawedBack.Should().BeTrue();
            _sutModel.ClawbackPaymentModels.Count.Should().Be(1);
        }

        private ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentive.Get(model.Id, model);
        }
    }
}