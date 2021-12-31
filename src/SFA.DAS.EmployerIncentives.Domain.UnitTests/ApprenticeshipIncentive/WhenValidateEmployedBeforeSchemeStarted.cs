using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
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
            var validationResult = _sut.PendingPayments.First().PendingPaymentValidationResults.FirstOrDefault(x => x.Step == "EmployedBeforeSchemeStarted");
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
            var validationResult = _sut.PendingPayments.First().PendingPaymentValidationResults.FirstOrDefault(x => x.Step == "EmployedBeforeSchemeStarted");
            validationResult.Should().NotBeNull();
            validationResult.Result.Should().BeFalse();
        }

        [Test]
        public void Then_a_true_validation_result_is_created_when_the_employment_check_record_returns_false()
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
                    new EmploymentCheckModel { CheckType = EmploymentCheckType.EmployedBeforeSchemeStarted, Result = false }
                })
                .Create();

            _sut = Sut(_sutModel);

            // Act
            _sut.ValidateEmploymentChecks(_sutModel.PendingPaymentModels.First().Id, _collectionPeriod);

            // Assert
            var validationResult = _sut.PendingPayments.First().PendingPaymentValidationResults.FirstOrDefault(x => x.Step == "EmployedBeforeSchemeStarted");
            validationResult.Should().NotBeNull();
            validationResult.Result.Should().BeTrue();
        }

        [Test]
        public void Then_a_false_validation_result_is_created_when_the_employment_check_record_returns_true()
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
                    new EmploymentCheckModel { CheckType = EmploymentCheckType.EmployedBeforeSchemeStarted, Result = true }
                })
                .Create();

            _sut = Sut(_sutModel);

            // Act
            _sut.ValidateEmploymentChecks(_sutModel.PendingPaymentModels.First().Id, _collectionPeriod);

            // Assert
            var validationResult = _sut.PendingPayments.First().PendingPaymentValidationResults.FirstOrDefault(x => x.Step == "EmployedBeforeSchemeStarted");
            validationResult.Should().NotBeNull();
            validationResult.Result.Should().BeFalse();
        }

        private ApprenticeshipIncentives.ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentives.ApprenticeshipIncentive.Get(model.Id, model);
        }
    }
}
