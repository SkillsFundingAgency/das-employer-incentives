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
        private long _apprenticeshipId;
        private DateTime _startDate;
        private DateTime _endDate;
        private byte _periodNumber;
        private PendingPayment _pendingPayment;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sut = _fixture.Create<LearnerSubmissionDto>();
            _apprenticeshipId = _fixture.Create<long>();
            _startDate = DateTime.Now;
            _endDate = _startDate.AddMonths(2);
            _periodNumber = 3;

            var pendingPaymentModel = _fixture
                .Build<PendingPaymentModel>()
                .With(p => p.DueDate, _startDate.AddMonths(1))
                .Create();
            var apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(p => p.Apprenticeship, _fixture.Create<Apprenticeship>())
                .With(p => p.PendingPaymentModels, new List<PendingPaymentModel> {
                    _fixture.Create<PendingPaymentModel>(),
                    pendingPaymentModel,
                    _fixture.Create<PendingPaymentModel>()
                    })
                .Create();

            var incentive = new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.Id, apprenticeshipIncentiveModel);
            _pendingPayment = incentive.PendingPayments.Single(p => p.Id == pendingPaymentModel.Id);

            _apprenticeshipId = apprenticeshipIncentiveModel.Apprenticeship.Id;

            _testTrainingDto = _sut.Training.First();
            _testTrainingDto.Reference = "ZPROG001";

            _testPriceEpisodeDto = _testTrainingDto.PriceEpisodes.First();
            _testPriceEpisodeDto.StartDate = _startDate;
            _testPriceEpisodeDto.EndDate = _endDate;

            _testPeriodDto = _testPriceEpisodeDto.Periods.First();            

            _testPeriodDto.ApprenticeshipId = _apprenticeshipId;
            _testPeriodDto.Period = _periodNumber;
            _testPeriodDto.IsPayable = true;
        }


        [Test]
        public void Then_true_is_returned_when_there_is_a_matching_period()
        {
            //Arrange    

            //Act
            var isInLearning = _sut.InLearningForAppenticeship(_apprenticeshipId, _pendingPayment);

            //Assert
            isInLearning.Should().BeTrue();
        }

        [Test]
        public void Then_true_is_returned_when_there_is_a_matching_period_with_a_null_end_date()
        {
            //Arrange    
            _testPriceEpisodeDto.EndDate = null;

            //Act
            var isInLearning = _sut.InLearningForAppenticeship(_apprenticeshipId, _pendingPayment);

            //Assert
            isInLearning.Should().BeTrue();
        }

        [Test]
        public void Then_false_is_returned_when_there_are_no_matching_periods()
        {
            //Arrange    
            _testTrainingDto.Reference = _fixture.Create<string>();

            //Act
            var isInLearning = _sut.InLearningForAppenticeship(_apprenticeshipId, _pendingPayment);

            //Assert
            isInLearning.Should().BeFalse();
        }

        [Test]
        public void Then_false_is_returned_when_the_due_date_is_before_a_matching_period()
        {
            //Arrange    
            _testPriceEpisodeDto.StartDate = _pendingPayment.DueDate.AddSeconds(1);

            //Act
            var isInLearning = _sut.InLearningForAppenticeship(_apprenticeshipId, _pendingPayment);

            //Assert
            isInLearning.Should().BeFalse();
        }

        [Test]
        public void Then_false_is_returned_when_the_apprenticeship_id_does_not_have_a_matching_period()
        {
            //Arrange    
            _testPeriodDto.ApprenticeshipId = _fixture.Create<long>();

            //Act
            var isInLearning = _sut.InLearningForAppenticeship(_apprenticeshipId, _pendingPayment);

            //Assert
            isInLearning.Should().BeFalse();
        }

        [Test]
        public void Then_false_is_returned_when_the_due_date_is_after_a_matching_period()
        {
            //Arrange    
            _testPriceEpisodeDto.EndDate = _pendingPayment.DueDate.AddSeconds(-1);

            //Act
            var isInLearning = _sut.InLearningForAppenticeship(_apprenticeshipId, _pendingPayment);

            //Assert
            isInLearning.Should().BeFalse();
        }


        [Test]
        public void Then_false_is_returned_when_the_pending_payment_is_null()
        {
            //Arrange    

            //Act
            var isInLearning = _sut.InLearningForAppenticeship(_apprenticeshipId, null);

            //Assert
            isInLearning.Should().BeFalse();
        }
    }
}
