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
    public class WhenValidateIsInLearning
    {
        private ApprenticeshipIncentive _sut;
        private ApprenticeshipIncentiveModel _sutModel;
        private CollectionPeriod _collectionPeriod;
        private Learner _learner;
        private short _collectionYear;
        private byte _collectionMonth;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _collectionYear = _fixture.Create<short>();
            _collectionMonth = _fixture.Create<byte>();

            _collectionPeriod = new CollectionPeriod(1, _collectionMonth, _collectionYear, DateTime.Now, DateTime.Now, _collectionYear, true);

            _fixture.Build<PendingPaymentModel>().With(p => p.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create();

            _sutModel = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .With(a => a.PendingPaymentModels, new List<PendingPaymentModel>() 
                    {
                        _fixture.Build<PendingPaymentModel>().With(p => p.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create()
                    })
                .Create();

            _sutModel.Apprenticeship.SetProvider(_fixture.Create<Provider>());

            _learner = Learner.New(
                Guid.NewGuid(),
                _sutModel.Id,
                _sutModel.Apprenticeship.Id,
                _sutModel.Apprenticeship.Provider.Ukprn,
                _sutModel.Apprenticeship.UniqueLearnerNumber);

            var submisssionData = new SubmissionData();
            submisssionData.SetSubmissionDate(DateTime.Now);
            _learner.SetSubmissionData(submisssionData);

            _sut = Sut(_sutModel);
        }

        [TestCase(false)]
        [TestCase(true)]        
        public void Then_a_validation_result_is_created_for_is_in_learning(bool isInLearning)
        {
            // arrange            
            var pendingPayment = _sut.PendingPayments.First();

            _learner.SubmissionData.SetLearningData(new LearningData(true));
            _learner.SubmissionData.LearningData.SetIsInLearning(isInLearning);

            // act
            _sut.ValidateIsInLearning(pendingPayment.Id, _learner, _collectionPeriod);

            // assert            
            pendingPayment.PendingPaymentValidationResults.Count.Should().Be(1);
            var validationresult = pendingPayment.PendingPaymentValidationResults.First();
            validationresult.Step.Should().Be(ValidationStep.IsInLearning);
            validationresult.CollectionPeriod.Should().Be(_collectionPeriod);
            validationresult.Result.Should().Be(isInLearning);
        }

        [Test()]
        public void Then_a_false_validation_result_is_created_when_the_matchedLearner_is_null()
        {
            // arrange            
            var pendingPayment = _sut.PendingPayments.First();

            // act
            _sut.ValidateIsInLearning(pendingPayment.Id, null, _collectionPeriod);

            // assert            
            pendingPayment.PendingPaymentValidationResults.Count.Should().Be(1);
            var validationresult = pendingPayment.PendingPaymentValidationResults.First();
            validationresult.Step.Should().Be(ValidationStep.IsInLearning);
            validationresult.CollectionPeriod.Should().Be(_collectionPeriod);
            validationresult.Result.Should().Be(false);
        }

        [Test()]
        public void Then_a_false_validation_result_is_created_when_the_matchedLearner_submissionFound_is_null()
        {
            // arrange            
            var pendingPayment = _sut.PendingPayments.First();

            _learner.SetSubmissionData(null);
            
            // act
            _sut.ValidateIsInLearning(pendingPayment.Id, _learner, _collectionPeriod);

            // assert            
            pendingPayment.PendingPaymentValidationResults.Count.Should().Be(1);
            var validationresult = pendingPayment.PendingPaymentValidationResults.First();
            validationresult.Step.Should().Be(ValidationStep.IsInLearning);
            validationresult.CollectionPeriod.Should().Be(_collectionPeriod);
            validationresult.Result.Should().Be(false);
        }

        [Test()]
        public void Then_a_false_validation_result_is_created_when_the_matchedLearner_IsInlearning_is_null()
        {
            // arrange            
            var pendingPayment = _sut.PendingPayments.First();

            _learner.SubmissionData.SetLearningData(new LearningData(true));
            _learner.SubmissionData.LearningData.SetIsInLearning(null);

            // act
            _sut.ValidateIsInLearning(pendingPayment.Id, _learner, _collectionPeriod);

            // assert            
            pendingPayment.PendingPaymentValidationResults.Count.Should().Be(1);
            var validationresult = pendingPayment.PendingPaymentValidationResults.First();
            validationresult.Step.Should().Be(ValidationStep.IsInLearning);
            validationresult.CollectionPeriod.Should().Be(_collectionPeriod);
            validationresult.Result.Should().Be(false);
        }

        private ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentive.Get(model.Id, model);
        }
    }
}
