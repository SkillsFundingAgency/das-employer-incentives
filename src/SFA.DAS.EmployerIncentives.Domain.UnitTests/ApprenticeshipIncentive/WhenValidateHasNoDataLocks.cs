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
    public class WhenValidateHasNoDataLocks
    {
        private ApprenticeshipIncentives.ApprenticeshipIncentive _sut;
        private ApprenticeshipIncentiveModel _sutModel;
        private CollectionPeriod _collectionPeriod;
        private Learner _learner;
        private short _collectionYear;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _collectionYear = _fixture.Create<short>();

            _collectionPeriod = new CollectionPeriod(1, _collectionYear);

            _fixture.Build<PendingPaymentModel>().With(p => p.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create();

            _sutModel = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .With(a => a.PendingPaymentModels, new List<PendingPaymentModel>() 
                    {
                        _fixture.Build<PendingPaymentModel>().With(p => p.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create()
                    })
                .Create();

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
        public void Then_a_validation_result_is_created_for_has_no_datalocks(bool hasDataLock)
        {
            // arrange            
            var pendingPayment = _sut.PendingPayments.First();

            _learner.SubmissionData.SetLearningData(new LearningData(true));
            _learner.SubmissionData.LearningData.SetHasDataLock(hasDataLock);

            // act
            _sut.ValidateHasNoDataLocks(pendingPayment.Id, _learner, _collectionPeriod);

            // assert            
            pendingPayment.PendingPaymentValidationResults.Count.Should().Be(1);
            var validationresult = pendingPayment.PendingPaymentValidationResults.First();
            validationresult.Step.Should().Be(ValidationStep.HasNoDataLocks);
            validationresult.CollectionPeriod.Should().Be(_collectionPeriod);
            validationresult.Result.Should().Be(!hasDataLock);
            validationresult.GetModel().CreatedDateUtc.Should().Be(DateTime.Today);
        }

        [Test()]
        public void Then_a_false_validation_result_is_created_when_the_matchedLearner_is_null()
        {
            // arrange            
            var pendingPayment = _sut.PendingPayments.First();

            // act
            _sut.ValidateHasNoDataLocks(pendingPayment.Id, null, _collectionPeriod);

            // assert            
            pendingPayment.PendingPaymentValidationResults.Count.Should().Be(1);
            var validationresult = pendingPayment.PendingPaymentValidationResults.First();
            validationresult.Step.Should().Be(ValidationStep.HasNoDataLocks);
            validationresult.CollectionPeriod.Should().Be(_collectionPeriod);
            validationresult.Result.Should().Be(true);
            validationresult.GetModel().CreatedDateUtc.Should().Be(DateTime.Today);
        }

        [Test()]
        public void Then_a_false_validation_result_is_created_when_the_matchedLearner_submissionFound_is_null()
        {
            // arrange            
            var pendingPayment = _sut.PendingPayments.First();

            _learner.SetSubmissionData(null);
            
            // act
            _sut.ValidateHasNoDataLocks(pendingPayment.Id, _learner, _collectionPeriod);

            // assert            
            pendingPayment.PendingPaymentValidationResults.Count.Should().Be(1);
            var validationresult = pendingPayment.PendingPaymentValidationResults.First();
            validationresult.Step.Should().Be(ValidationStep.HasNoDataLocks);
            validationresult.CollectionPeriod.Should().Be(_collectionPeriod);
            validationresult.Result.Should().Be(true);
            validationresult.GetModel().CreatedDateUtc.Should().Be(DateTime.Today);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Then_a_validation_result_is_overridden_for_for_has_no_datalocks_when_an_non_expired_override_exists(bool hasDataLock)
        {
            // arrange            
            var pendingPayment = _sut.PendingPayments.First();

            _learner.SubmissionData.SetLearningData(new LearningData(true));
            _learner.SubmissionData.LearningData.SetHasDataLock(hasDataLock);
            _sut.AddValidationOverride(new ValidationOverrideStep(ValidationStep.HasNoDataLocks, DateTime.Now.AddDays(1)), _fixture.Create<ServiceRequest>());

            // act
            _sut.ValidateHasNoDataLocks(pendingPayment.Id, _learner, _collectionPeriod);

            // assert            
            pendingPayment.PendingPaymentValidationResults.Count.Should().Be(1);
            var validationresult = pendingPayment.PendingPaymentValidationResults.First();
            validationresult.Step.Should().Be(ValidationStep.HasNoDataLocks);
            validationresult.CollectionPeriod.Should().Be(_collectionPeriod);
            validationresult.Result.Should().BeTrue();
            validationresult.GetModel().CreatedDateUtc.Should().Be(DateTime.Today);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Then_a_validation_result_is_overridden_for_for_has_no_datalocks_when_an_expired_override_exists(bool hasDataLock)
        {
            // arrange            
            var pendingPayment = _sut.PendingPayments.First();

            _learner.SubmissionData.SetLearningData(new LearningData(true));
            _learner.SubmissionData.LearningData.SetHasDataLock(hasDataLock);
            _sut.AddValidationOverride(new ValidationOverrideStep(ValidationStep.HasNoDataLocks, DateTime.Now), _fixture.Create<ServiceRequest>());

            // act
            _sut.ValidateHasNoDataLocks(pendingPayment.Id, _learner, _collectionPeriod);

            // assert            
            pendingPayment.PendingPaymentValidationResults.Count.Should().Be(1);
            var validationresult = pendingPayment.PendingPaymentValidationResults.First();
            validationresult.Step.Should().Be(ValidationStep.HasNoDataLocks);
            validationresult.CollectionPeriod.Should().Be(_collectionPeriod);
            validationresult.Result.Should().Be(!hasDataLock);
            validationresult.GetModel().CreatedDateUtc.Should().Be(DateTime.Today);
        }

        private ApprenticeshipIncentives.ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentives.ApprenticeshipIncentive.Get(model.Id, model);
        }
    }
}
