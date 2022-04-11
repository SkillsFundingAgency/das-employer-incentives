using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests
{
    [TestFixture]
    public class WhenValidateEmployedBeforeSchemeStarted
    {
        private ApprenticeshipIncentives.ApprenticeshipIncentive _sut;
        private ApprenticeshipIncentiveModel _sutModel;
        private CollectionPeriod _collectionPeriod;
        private short _collectionYear;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _collectionYear = _fixture.Create<short>();

            _collectionPeriod = new CollectionPeriod(1, _collectionYear);

        }


        [Test]
        public void Then_a_false_validation_result_is_created_when_the_employment_check_record_does_not_exist()
        {
            // Arrange
            _sutModel = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .With(x => x.Phase, new IncentivePhase(Phase.Phase2))
                .With(a => a.PendingPaymentModels, new List<PendingPaymentModel>()
                {
                    _fixture.Build<PendingPaymentModel>()
                        .With(p => p.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create()
                })
                .With(x => x.EmploymentCheckModels, new List<EmploymentCheckModel>()
                {
                })
                .Create();

            _sut = Sut(_sutModel);

            // Act
            _sut.ValidateEmploymentChecks(_sutModel.PendingPaymentModels.First().Id, _collectionPeriod);

            // Assert
            var validationResult = _sut.PendingPayments.First().PendingPaymentValidationResults.FirstOrDefault(x => x.Step == ValidationStep.EmployedBeforeSchemeStarted);
            validationResult.Should().NotBeNull();
            validationResult.Result.Should().BeFalse();
        }

        [Test]
        public void Then_a_false_validation_result_is_created_when_the_employment_check_record_returns_no_result()
        {
            // Arrange
            _sutModel = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .With(x => x.Phase, new IncentivePhase(Phase.Phase2))
                .With(a => a.PendingPaymentModels, new List<PendingPaymentModel>()
                {
                    _fixture.Build<PendingPaymentModel>()
                        .With(p => p.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create()
                })
                .With(x => x.EmploymentCheckModels, new List<EmploymentCheckModel>()
                {
                    new EmploymentCheckModel { CheckType = EmploymentCheckType.EmployedBeforeSchemeStarted, Result = null }
                })
                .Create();

            _sut = Sut(_sutModel);

            // Act
            _sut.ValidateEmploymentChecks(_sutModel.PendingPaymentModels.First().Id, _collectionPeriod);

            // Assert
            var validationResult = _sut.PendingPayments.First().PendingPaymentValidationResults.FirstOrDefault(x => x.Step == ValidationStep.EmployedBeforeSchemeStarted);
            validationResult.Should().NotBeNull();
            validationResult.Result.Should().BeFalse();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Then_a_validation_result_is_created_for_the_employed_before_scheme_started(bool employedBeforeSchemeStarted)
        {
            // Arrange
            _sutModel = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .With(x => x.Phase, new IncentivePhase(Phase.Phase2))
                .With(a => a.PendingPaymentModels, new List<PendingPaymentModel>()
                {
                    _fixture.Build<PendingPaymentModel>()
                        .With(p => p.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create()
                })
                .With(x => x.EmploymentCheckModels, new List<EmploymentCheckModel>()
                {
                    new EmploymentCheckModel { CheckType = EmploymentCheckType.EmployedBeforeSchemeStarted, Result = employedBeforeSchemeStarted }
                })
                .Create();

            _sut = Sut(_sutModel);

            // Act
            _sut.ValidateEmploymentChecks(_sutModel.PendingPaymentModels.First().Id, _collectionPeriod);

            // Assert
            var validationResult = _sut.PendingPayments.First().PendingPaymentValidationResults.FirstOrDefault(x => x.Step == ValidationStep.EmployedBeforeSchemeStarted);
            validationResult.Should().NotBeNull();
            validationResult.Result.Should().Be(!employedBeforeSchemeStarted);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Then_a_validation_result_is_overridden_for_the_employed_before_scheme_started_when_an_non_expired_override_exists(bool employedBeforeSchemeStarted)
        {
            // Arrange
            _sutModel = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .With(x => x.Phase, new IncentivePhase(Phase.Phase2))
                .With(a => a.PendingPaymentModels, new List<PendingPaymentModel>()
                {
                    _fixture.Build<PendingPaymentModel>()
                        .With(p => p.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create()
                })
                .With(x => x.EmploymentCheckModels, new List<EmploymentCheckModel>()
                {
                    new EmploymentCheckModel { CheckType = EmploymentCheckType.EmployedBeforeSchemeStarted, Result = employedBeforeSchemeStarted }
                })
                .Create();

            _sut = Sut(_sutModel);

            _sut.AddValidationOverride(new ValidationOverrideStep(ValidationStep.EmployedBeforeSchemeStarted, DateTime.Now.AddDays(1)), _fixture.Create<ServiceRequest>());

            // Act
            _sut.ValidateEmploymentChecks(_sutModel.PendingPaymentModels.First().Id, _collectionPeriod);

            // Assert
            var validationResult = _sut.PendingPayments.First().PendingPaymentValidationResults.FirstOrDefault(x => x.Step == ValidationStep.EmployedBeforeSchemeStarted);
            validationResult.Should().NotBeNull();
            validationResult.Result.Should().BeTrue();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Then_a_validation_result_is_not_overridden_for_the_employed_before_scheme_started_when_an_expired_override_exists(bool employedBeforeSchemeStarted)
        {
            // Arrange
            _sutModel = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .With(x => x.Phase, new IncentivePhase(Phase.Phase2))
                .With(a => a.PendingPaymentModels, new List<PendingPaymentModel>()
                {
                    _fixture.Build<PendingPaymentModel>()
                        .With(p => p.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create()
                })
                .With(x => x.EmploymentCheckModels, new List<EmploymentCheckModel>()
                {
                    new EmploymentCheckModel { CheckType = EmploymentCheckType.EmployedBeforeSchemeStarted, Result = employedBeforeSchemeStarted }
                })
                .Create();

            _sut = Sut(_sutModel);

            _sut.AddValidationOverride(new ValidationOverrideStep(ValidationStep.EmployedBeforeSchemeStarted, DateTime.Now), _fixture.Create<ServiceRequest>());

            // Act
            _sut.ValidateEmploymentChecks(_sutModel.PendingPaymentModels.First().Id, _collectionPeriod);

            // Assert
            var validationResult = _sut.PendingPayments.First().PendingPaymentValidationResults.FirstOrDefault(x => x.Step == ValidationStep.EmployedBeforeSchemeStarted);
            validationResult.Should().NotBeNull();
            validationResult.Result.Should().Be(!employedBeforeSchemeStarted);
        }

        private ApprenticeshipIncentives.ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentives.ApprenticeshipIncentive.Get(model.Id, model);
        }
    }
}
