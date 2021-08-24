using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests
{
    public class WhenValidateDaysInLearning
    {
        private ApprenticeshipIncentives.ApprenticeshipIncentive _sut;
        private ApprenticeshipIncentiveModel _sutModel;
        private CollectionPeriod _collectionPeriod;
        private Learner _learner;
        private LearnerModel _learnerModel;
        private short _collectionYear;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _collectionYear = _fixture.Create<short>();

            _collectionPeriod = new CollectionPeriod(1, _collectionYear);

            var startDate = DateTime.Now.Date;
            var dueDate = startDate.AddDays(90).Date;

            _sutModel = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .With(a => a.StartDate, startDate)
                .With(a => a.PendingPaymentModels, new List<PendingPaymentModel>() 
                    {
                        _fixture.Build<PendingPaymentModel>()
                        .With(p => p.DueDate, dueDate)
                        .With(p => p.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create()
                    })                
                .Create();

            _sutModel.Apprenticeship.SetProvider(_fixture.Create<Provider>());

            _learnerModel = _fixture
                .Build<LearnerModel>()
                .With(l => l.DaysInLearnings, new List<DaysInLearning>() { new DaysInLearning(_collectionPeriod, 90) })
                .Create();

            _learner = Learner.Get(_learnerModel);                

            _sut = Sut(_sutModel);
        }

        [Test()]
        public void Then_a_false_validation_result_is_created_when_the_matchedLearner_is_null()
        {
            // arrange            
            var pendingPayment = _sut.PendingPayments.First();

            // act
            _sut.ValidateDaysInLearning(pendingPayment.Id, null, _collectionPeriod);

            // assert            
            pendingPayment.PendingPaymentValidationResults.Count.Should().Be(1);
            var validationresult = pendingPayment.PendingPaymentValidationResults.First();
            validationresult.Step.Should().Be(ValidationStep.HasDaysInLearning);
            validationresult.CollectionPeriod.Should().Be(_collectionPeriod);
            validationresult.Result.Should().Be(false);
            validationresult.GetModel().CreatedDateUtc.Should().Be(DateTime.Today);
        }

        [TestCase(89, false)]
        [TestCase(90, true)]
        [TestCase(91, true)]
        public void Then_a_validation_result_is_created_for_the_days_in_learning(int daysInLearning, bool validationResult)
        {
            // arrange            
            var pendingPayment = _sut.PendingPayments.First();

            _learnerModel.DaysInLearnings = new List<DaysInLearning>() {
                new DaysInLearning(_collectionPeriod, daysInLearning)
            };

            _learner = Learner.Get(_learnerModel);

            // act
            _sut.ValidateDaysInLearning(pendingPayment.Id, _learner, _collectionPeriod);

            // assert            
            pendingPayment.PendingPaymentValidationResults.Count.Should().Be(1);
            var validationresult = pendingPayment.PendingPaymentValidationResults.First();
            validationresult.Step.Should().Be(ValidationStep.HasDaysInLearning);
            validationresult.CollectionPeriod.Should().Be(_collectionPeriod);
            validationresult.Result.Should().Be(validationResult);
            validationresult.GetModel().CreatedDateUtc.Should().Be(DateTime.Today);
        }
        
        private ApprenticeshipIncentives.ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentives.ApprenticeshipIncentive.Get(model.Id, model);
        }
    }
}
