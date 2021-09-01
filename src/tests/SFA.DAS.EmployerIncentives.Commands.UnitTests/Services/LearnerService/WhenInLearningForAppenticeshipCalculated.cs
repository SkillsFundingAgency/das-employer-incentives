using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.Services.LearnerServiceTests
{
    public class WhenInLearningForAppenticeshipCalculated
    {
        private LearnerSubmissionDto _sut;
        private TrainingDto _testTrainingDto;
        private PriceEpisodeDto _testPriceEpisodeDto;
        private PeriodDto _testPeriodDto;
        private DateTime _startDate;
        private DateTime _endDate;
        private byte _periodNumber;
        private PendingPayment _pendingPayment;
        private Domain.ApprenticeshipIncentives.ApprenticeshipIncentive _incentive;
        private ApprenticeshipIncentiveModel _apprenticeshipIncentiveModel;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sut = _fixture.Create<LearnerSubmissionDto>();            
            _startDate = DateTime.Now;
            _endDate = _startDate.AddMonths(2);
            _periodNumber = 3;
            var dueDate = _startDate.AddMonths(1);

            var pendingPaymentModel = _fixture
                .Build<PendingPaymentModel>()
                .With(pp => pp.PaymentMadeDate, (DateTime?)null)
                .With(p => p.DueDate, dueDate)
                .Create();
            _apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(p => p.Apprenticeship, _fixture.Create<Apprenticeship>())
                .With(p => p.PendingPaymentModels, new List<PendingPaymentModel> {
                     _fixture.Build<PendingPaymentModel>()
                    .With(pp => pp.PaymentMadeDate, (DateTime?)null)
                    .With(pp => pp.DueDate, dueDate.AddMonths(1))
                    .With(pp => pp.CollectionPeriod, (Domain.ValueObjects.CollectionPeriod)null)
                    .Create(),
                    _fixture.Build<PendingPaymentModel>()
                    .With(pp => pp.PaymentMadeDate, (DateTime?)null)
                    .With(pp => pp.DueDate, dueDate.AddMonths(2))
                    .With(pp => pp.CollectionPeriod, (Domain.ValueObjects.CollectionPeriod)null)
                    .Create(),
                    pendingPaymentModel
                    })
                .Create();

            _incentive = new ApprenticeshipIncentiveFactory().GetExisting(_apprenticeshipIncentiveModel.Id, _apprenticeshipIncentiveModel);
            _pendingPayment = _incentive.PendingPayments.Single(p => p.Id == pendingPaymentModel.Id);

            _testTrainingDto = _sut.Training.First();
            _testTrainingDto.Reference = "ZPROG001";

            _testPriceEpisodeDto = _testTrainingDto.PriceEpisodes.First();
            _testPriceEpisodeDto.StartDate = _startDate;
            _testPriceEpisodeDto.EndDate = _endDate;

            _testPeriodDto = _testPriceEpisodeDto.Periods.First();            

            _testPeriodDto.ApprenticeshipId = _apprenticeshipIncentiveModel.Apprenticeship.Id;
            _testPeriodDto.Period = _periodNumber;
            _testPeriodDto.IsPayable = true;
        }


        [Test]
        public void Then_true_is_returned_when_there_is_a_matching_period()
        {
            //Arrange    

            //Act
            var isInLearning = _sut.IsInLearning(_incentive);

            //Assert
            isInLearning.Should().BeTrue();
        }

        [Test]
        public void Then_true_is_returned_when_there_is_a_matching_period_with_a_null_end_date()
        {
            //Arrange    
            _testPriceEpisodeDto.EndDate = null;

            //Act
            var isInLearning = _sut.IsInLearning(_incentive);

            //Assert
            isInLearning.Should().BeTrue();
        }

        [Test]
        public void Then_false_is_returned_when_there_are_no_matching_periods()
        {
            //Arrange    
            _testTrainingDto.Reference = _fixture.Create<string>();

            //Act
            var isInLearning = _sut.IsInLearning(_incentive);

            //Assert
            isInLearning.Should().BeFalse();
        }

        [Test]
        public void Then_false_is_returned_when_the_due_date_is_before_a_matching_period()
        {
            //Arrange    
            _testPriceEpisodeDto.StartDate = _pendingPayment.DueDate.AddDays(1);            

            //Act
            var isInLearning = _sut.IsInLearning(_incentive);

            //Assert
            isInLearning.Should().BeFalse();
        }

        [Test]
        public void Then_false_is_returned_when_the_apprenticeship_id_does_not_have_a_matching_period()
        {
            //Arrange    
            _testPeriodDto.ApprenticeshipId = _fixture.Create<long>();

            //Act
            var isInLearning = _sut.IsInLearning(_incentive);

            //Assert
            isInLearning.Should().BeFalse();
        }

        [Test]
        public void Then_false_is_returned_when_the_due_date_is_after_a_matching_period()
        {
            //Arrange    
            _testPriceEpisodeDto.EndDate = _pendingPayment.DueDate.AddSeconds(-1);

            //Act
            var isInLearning = _sut.IsInLearning(_incentive);

            //Assert
            isInLearning.Should().BeFalse();
        }


        [Test]
        public void Then_false_is_returned_when_the_pending_payment_is_null()
        {
            //Arrange    
            _apprenticeshipIncentiveModel.PendingPaymentModels.Clear();

            _incentive = new ApprenticeshipIncentiveFactory().GetExisting(_apprenticeshipIncentiveModel.Id, _apprenticeshipIncentiveModel);

            //Act
            var isInLearning = _sut.IsInLearning(_incentive);

            //Assert
            isInLearning.Should().BeFalse();
        }
    }
}
