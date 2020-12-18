using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.Services.LearnerServiceTests
{
    public class WhenHasProviderDataLocks
    {
        private LearnerSubmissionDto _sut;
        private TrainingDto _testTrainingDto;
        private PriceEpisodeDto _testPriceEpisodeDto;
        private PeriodDto _testPeriodDto;
        private Domain.ApprenticeshipIncentives.ApprenticeshipIncentive _incentive;
        private ApprenticeshipIncentiveModel _apprenticeshipIncentiveModel;
        private PendingPaymentModel _nextPendingPaymentDue;
        private DateTime _startTime;
        private Fixture _fixture;
        private DateTime _dueDate;
        private byte _periodNumber;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _dueDate = DateTime.Now.AddMonths(-1);
            short paymentYear = (short)DateTime.Now.Year;
            _periodNumber = _fixture.Create<byte>();

            _nextPendingPaymentDue = _fixture.Build<PendingPaymentModel>()
                    .With(pp => pp.PaymentMadeDate, (DateTime?)null)
                    .With(pp => pp.PaymentYear, paymentYear)
                    .With(pp => pp.PeriodNumber, _periodNumber)
                    .With(pp => pp.DueDate, _dueDate)// earliest
                    .Create();

            _apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(a => a.PendingPaymentModels, new List<PendingPaymentModel>()
                {
                    _fixture.Build<PendingPaymentModel>()
                    .With(pp => pp.PaymentMadeDate, (DateTime?)null)
                    .With(pp => pp.DueDate, _dueDate.AddMonths(1))
                    .With(pp => pp.PaymentYear, (short?)null)
                    .Create(),
                    _fixture.Build<PendingPaymentModel>()
                    .With(pp => pp.PaymentMadeDate, (DateTime?)null)
                    .With(pp => pp.DueDate, _dueDate.AddMonths(2))
                    .With(pp => pp.PaymentYear, (short?)null)
                    .Create(),
                    _nextPendingPaymentDue
                })
                .Create();
            
            _incentive = new ApprenticeshipIncentiveFactory().GetExisting(_apprenticeshipIncentiveModel.Id, _apprenticeshipIncentiveModel);

            _sut = _fixture.Create<LearnerSubmissionDto>();
            _startTime = _dueDate.AddDays(-10);

            _testTrainingDto = _sut.Training.First();
            _testTrainingDto.Reference = "ZPROG001";

            _testPriceEpisodeDto = _testTrainingDto.PriceEpisodes.First();
            _testPriceEpisodeDto.StartDate = _startTime;
            _testPriceEpisodeDto.EndDate = _dueDate.AddDays(10);

            _testPeriodDto = _testPriceEpisodeDto.Periods.First();

            _testPeriodDto.ApprenticeshipId = _incentive.Apprenticeship.Id;
            _testPeriodDto.IsPayable = false;
            _testPeriodDto.Period = _periodNumber;
        }


        [Test]
        public void Then_is_true_when_there_is_a_matching_record()
        {
            //Arrange            

            //Act
            var hasDataLock = _sut.HasProviderDataLocks(_incentive);

            //Assert
            hasDataLock.Should().BeTrue();
        }

        [Test]
        public void Then_is_true_when_there_is_a_matching_record_with_a_null_period_end_date()
        {
            //Arrange            
            _testPriceEpisodeDto.EndDate = null;

            //Act
            var hasDataLock = _sut.HasProviderDataLocks(_incentive);

            //Assert
            hasDataLock.Should().BeTrue();
        }

        [Test]
        public void Then_is_false_when_the_incentive_is_null()
        {
            //Arrange            
            _incentive = null;

            //Act
            var hasDataLock = _sut.HasProviderDataLocks(_incentive);

            //Assert
            hasDataLock.Should().BeFalse();
        }

        [Test]
        public void Then_is_false_when_the_incentive_has_no_due_pending_payments()
        {
            //Arrange            
            _apprenticeshipIncentiveModel.PendingPaymentModels.ToList().ForEach(p => p.PaymentMadeDate = _fixture.Create<DateTime>());
            _incentive = new ApprenticeshipIncentiveFactory().GetExisting(_apprenticeshipIncentiveModel.Id, _apprenticeshipIncentiveModel);

            //Act
            var hasDataLock = _sut.HasProviderDataLocks(_incentive);

            //Assert
            hasDataLock.Should().BeFalse();
        }

        [Test]
        public void Then_is_false_when_the_next_pending_payment_due_has_a_null_payment_year()
        {
            //Arrange            
            _nextPendingPaymentDue.PaymentYear = null;
            _incentive = new ApprenticeshipIncentiveFactory().GetExisting(_apprenticeshipIncentiveModel.Id, _apprenticeshipIncentiveModel);

            //Act
            var hasDataLock = _sut.HasProviderDataLocks(_incentive);

            //Assert
            hasDataLock.Should().BeFalse();
        }

        [Test]
        public void Then_is_false_when_the_next_pending_payment_due_has_a_null_periodNumber()
        {
            //Arrange            
            _nextPendingPaymentDue.PeriodNumber = null;
            _incentive = new ApprenticeshipIncentiveFactory().GetExisting(_apprenticeshipIncentiveModel.Id, _apprenticeshipIncentiveModel);

            //Act
            var hasDataLock = _sut.HasProviderDataLocks(_incentive);

            //Assert
            hasDataLock.Should().BeFalse();
        }

        [Test]
        public void Then_is_false_when_there_are_no_matching_training_records_with_a_matching_reference()
        {
            //Arrange            
            _testTrainingDto.Reference = _fixture.Create<string>();

            //Act
            var hasDataLock = _sut.HasProviderDataLocks(_incentive);

            //Assert
            hasDataLock.Should().BeFalse();
        }

        [Test]
        public void Then_is_false_when_there_are_no_matching_training_records_priceEpisodes_a_startdate_less_than_the_payment_due_date()
        {
            //Arrange            
            _testPriceEpisodeDto.StartDate = _dueDate.AddDays(1);

            //Act
            var hasDataLock = _sut.HasProviderDataLocks(_incentive);

            //Assert
            hasDataLock.Should().BeFalse();
        }

        [Test]
        public void Then_is_false_when_there_are_no_matching_training_records_priceEpisodes_an_enddate_greater_than_the_payment_due_date()
        {
            //Arrange            
            _testPriceEpisodeDto.EndDate = _dueDate.AddDays(-1);

            //Act
            var hasDataLock = _sut.HasProviderDataLocks(_incentive);

            //Assert
            hasDataLock.Should().BeFalse();
        }

        [Test]
        public void Then_is_false_when_there_are_no_matching_training_records_priceEpisodes_with_a_period_that_has_a_matching_apprenticeship()
        {
            //Arrange            
            _testPeriodDto.ApprenticeshipId = _incentive.Apprenticeship.Id + 1;

            //Act
            var hasDataLock = _sut.HasProviderDataLocks(_incentive);

            //Assert
            hasDataLock.Should().BeFalse();
        }

        [Test]
        public void Then_is_false_when_there_are_no_matching_training_records_priceEpisodes_with_a_period_that_has_a_matching_period()
        {
            //Arrange            
            _testPeriodDto.Period = _fixture.Create<byte>();

            //Act
            var hasDataLock = _sut.HasProviderDataLocks(_incentive);

            //Assert
            hasDataLock.Should().BeFalse();
        }

        [Test]
        public void Then_is_false_when_there_are_no_matching_training_records_priceEpisodes_with_a_period_that_is_payable()
        {
            //Arrange            
            _testPeriodDto.IsPayable = true;

            //Act
            var hasDataLock = _sut.HasProviderDataLocks(_incentive);

            //Assert
            hasDataLock.Should().BeFalse();
        }
    }
}
