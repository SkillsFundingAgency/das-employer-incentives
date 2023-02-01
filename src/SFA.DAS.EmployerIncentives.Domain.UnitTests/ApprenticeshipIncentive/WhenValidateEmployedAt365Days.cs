using System;
using System.Collections.Generic;
using System.Linq;
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
    public class WhenValidateEmployedAt365Days
    {
        private ApprenticeshipIncentives.ApprenticeshipIncentive _sut;
        private ApprenticeshipIncentiveModel _sutModel;
        private CollectionPeriod _collectionPeriod;
        private short _collectionYear;
        private Fixture _fixture;
        private Mock<IDateTimeService> _mockDateTimeService;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _collectionYear = _fixture.Create<short>();

            _collectionPeriod = new CollectionPeriod(1, _collectionYear);

            _mockDateTimeService = new Mock<IDateTimeService>();
        }

        [Test]
        public void Then_the_validation_is_false_if_the_payment_due_date_is_not_in_range()
        {
            // Arrange
            var currentDateTime = DateTime.UtcNow;
            _mockDateTimeService.Setup(x => x.UtcNow()).Returns(currentDateTime);

            _sutModel = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .With(x => x.Phase, new IncentivePhase(Phase.Phase3))
                .With(a => a.PendingPaymentModels, new List<PendingPaymentModel>()
                {
                    _fixture.Build<PendingPaymentModel>()
                        .With(x => x.EarningType, EarningType.SecondPayment)
                        .With(p => p.DueDate, currentDateTime.AddDays(-21))
                        .With(p => p.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create()
                })
                .With(x => x.EmploymentCheckModels, new List<EmploymentCheckModel>()
                {
                })
                .Create();

            _sut = Sut(_sutModel);

            // Act
            _sut.ValidateEmploymentChecks(_mockDateTimeService.Object, _sutModel.PendingPaymentModels.First().Id, _collectionPeriod);
            
            // Assert
            var validationResult = _sut.PendingPayments.First().PendingPaymentValidationResults.FirstOrDefault(x => x.Step == ValidationStep.EmployedAt365Days);
            validationResult.Should().NotBeNull();
            validationResult.Result.Should().BeFalse();
        }

        [Test]
        public void Then_validation_is_skipped_for_the_first_earning()
        {
            // Arrange
            var currentDateTime = DateTime.UtcNow;
            _mockDateTimeService.Setup(x => x.UtcNow()).Returns(currentDateTime);

            _sutModel = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .With(x => x.Phase, new IncentivePhase(Phase.Phase3))
                .With(a => a.PendingPaymentModels, new List<PendingPaymentModel>()
                {
                    _fixture.Build<PendingPaymentModel>()
                        .With(x => x.EarningType, EarningType.FirstPayment)
                        .With(x => x.DueDate, currentDateTime.AddMonths(-1))
                        .With(p => p.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create()
                })
                .With(x => x.EmploymentCheckModels, new List<EmploymentCheckModel>()
                {
                })
                .Create();

            _sut = Sut(_sutModel);

            // Act
            _sut.ValidateEmploymentChecks(_mockDateTimeService.Object, _sutModel.PendingPaymentModels.First().Id, _collectionPeriod);

            // Assert
            var validationResult = _sut.PendingPayments.First().PendingPaymentValidationResults.FirstOrDefault(x => x.Step == ValidationStep.EmployedAt365Days);
            validationResult.Should().BeNull();
        }

        [Test]
        public void Then_a_false_validation_result_is_created_when_the_first_365_day_check_does_not_exist()
        {
            // Arrange
            var currentDateTime = DateTime.UtcNow;
            _mockDateTimeService.Setup(x => x.UtcNow()).Returns(currentDateTime);

            _sutModel = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .With(x => x.Phase, new IncentivePhase(Phase.Phase3))
                .With(a => a.PendingPaymentModels, new List<PendingPaymentModel>()
                {
                    _fixture.Build<PendingPaymentModel>()
                        .With(x => x.EarningType, EarningType.SecondPayment)
                        .With(p => p.DueDate, currentDateTime.AddDays(-22))
                        .With(p => p.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create()
                })
                .With(x => x.EmploymentCheckModels, new List<EmploymentCheckModel>()
                {
                })
                .Create();

            _sut = Sut(_sutModel);

            // Act
            _sut.ValidateEmploymentChecks(_mockDateTimeService.Object, _sutModel.PendingPaymentModels.First().Id, _collectionPeriod);

            // Assert
            var validationResult = _sut.PendingPayments.First().PendingPaymentValidationResults.FirstOrDefault(x => x.Step == ValidationStep.EmployedAt365Days);
            validationResult.Should().NotBeNull();
            validationResult.Result.Should().BeFalse();
        }

        [Test]
        public void Then_a_false_validation_result_is_created_when_the_first_365_day_check_record_returns_no_result()
        {
            // Arrange
            var currentDateTime = DateTime.UtcNow;
            _mockDateTimeService.Setup(x => x.UtcNow()).Returns(currentDateTime);

            _sutModel = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .With(x => x.Phase, new IncentivePhase(Phase.Phase3))
                .With(a => a.PendingPaymentModels, new List<PendingPaymentModel>()
                {
                    _fixture.Build<PendingPaymentModel>()
                        .With(x => x.EarningType, EarningType.SecondPayment)
                        .With(p => p.DueDate, currentDateTime.AddDays(-22))
                        .With(p => p.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create()
                })
                .With(x => x.EmploymentCheckModels, new List<EmploymentCheckModel>()
                {
                    new EmploymentCheckModel { CheckType = EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck, Result = null }
                })
                .Create();

            _sut = Sut(_sutModel);

            // Act
            _sut.ValidateEmploymentChecks(_mockDateTimeService.Object, _sutModel.PendingPaymentModels.First().Id, _collectionPeriod);

            // Assert
            var validationResult = _sut.PendingPayments.First().PendingPaymentValidationResults.FirstOrDefault(x => x.Step == ValidationStep.EmployedAt365Days);
            validationResult.Should().NotBeNull();
            validationResult.Result.Should().BeFalse();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Then_a_validation_result_is_created_for_the_first_365_day_check(bool employmentCheckResult)
        {
            // Arrange
            var currentDateTime = DateTime.UtcNow;
            _mockDateTimeService.Setup(x => x.UtcNow()).Returns(currentDateTime);

            _sutModel = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .With(x => x.Phase, new IncentivePhase(Phase.Phase3))
                .With(a => a.PendingPaymentModels, new List<PendingPaymentModel>()
                {
                    _fixture.Build<PendingPaymentModel>()
                        .With(x => x.EarningType, EarningType.SecondPayment)
                        .With(p => p.DueDate, currentDateTime.AddDays(-22))
                        .With(p => p.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create()
                })
                .With(x => x.EmploymentCheckModels, new List<EmploymentCheckModel>()
                {
                    new EmploymentCheckModel { CheckType = EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck, Result = employmentCheckResult }
                })
                .Create();

            _sut = Sut(_sutModel);

            // Act
            _sut.ValidateEmploymentChecks(_mockDateTimeService.Object, _sutModel.PendingPaymentModels.First().Id, _collectionPeriod);

            // Assert
            var validationResult = _sut.PendingPayments.First().PendingPaymentValidationResults.FirstOrDefault(x => x.Step == ValidationStep.EmployedAt365Days);
            validationResult.Should().NotBeNull();
            validationResult.Result.Should().Be(employmentCheckResult);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Then_a_validation_result_is_overridden_for_the_first_365_day_check_when_an_non_expired_override_exists(bool employmentCheckResult)
        {
            // Arrange
            var currentDateTime = DateTime.UtcNow;
            _mockDateTimeService.Setup(x => x.UtcNow()).Returns(currentDateTime);

            _sutModel = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .With(x => x.Phase, new IncentivePhase(Phase.Phase3))
                .With(a => a.PendingPaymentModels, new List<PendingPaymentModel>()
                {
                    _fixture.Build<PendingPaymentModel>()
                        .With(x => x.EarningType, EarningType.SecondPayment)
                        .With(p => p.DueDate, currentDateTime.AddDays(-22))
                        .With(p => p.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create()
                })
                .With(x => x.EmploymentCheckModels, new List<EmploymentCheckModel>()
                {
                    new EmploymentCheckModel { CheckType = EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck, Result = employmentCheckResult }
                })
                .Create();

            _sut = Sut(_sutModel);

            _sut.AddValidationOverride(new ValidationOverrideStep(ValidationStep.EmployedAt365Days, DateTime.Now.AddDays(1)), _fixture.Create<ServiceRequest>());

            // Act
            _sut.ValidateEmploymentChecks(_mockDateTimeService.Object, _sutModel.PendingPaymentModels.First().Id, _collectionPeriod);

            // Assert
            var validationResult = _sut.PendingPayments.First().PendingPaymentValidationResults.FirstOrDefault(x => x.Step == ValidationStep.EmployedAt365Days);
            validationResult.Should().NotBeNull();
            validationResult.Result.Should().BeTrue();
        }

        [Test]
        public void Then_a_false_validation_result_is_created_when_the_first_check_is_failed_and_the_second_does_not_exist()
        {
            // Arrange
            var currentDateTime = DateTime.UtcNow;
            _mockDateTimeService.Setup(x => x.UtcNow()).Returns(currentDateTime);

            _sutModel = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .With(x => x.Phase, new IncentivePhase(Phase.Phase3))
                .With(a => a.PendingPaymentModels, new List<PendingPaymentModel>()
                {
                    _fixture.Build<PendingPaymentModel>()
                        .With(x => x.EarningType, EarningType.SecondPayment)
                        .With(p => p.DueDate, currentDateTime.AddDays(-22))
                        .With(p => p.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create()
                })
                .With(x => x.EmploymentCheckModels, new List<EmploymentCheckModel>()
                {
                    new EmploymentCheckModel { CheckType = EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck, Result = false }
                })
                .Create();

            _sut = Sut(_sutModel);

            // Act
            _sut.ValidateEmploymentChecks(_mockDateTimeService.Object, _sutModel.PendingPaymentModels.First().Id, _collectionPeriod);

            // Assert
            var validationResult = _sut.PendingPayments.First().PendingPaymentValidationResults.FirstOrDefault(x => x.Step == ValidationStep.EmployedAt365Days);
            validationResult.Should().NotBeNull();
            validationResult.Result.Should().BeFalse();
        }

        [Test]
        public void Then_a_false_validation_result_is_created_when_the_first_check_is_failed_and_the_second_returns_no_result()
        {
            // Arrange
            var currentDateTime = DateTime.UtcNow;
            _mockDateTimeService.Setup(x => x.UtcNow()).Returns(currentDateTime);

            _sutModel = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .With(x => x.Phase, new IncentivePhase(Phase.Phase3))
                .With(a => a.PendingPaymentModels, new List<PendingPaymentModel>()
                {
                    _fixture.Build<PendingPaymentModel>()
                        .With(x => x.EarningType, EarningType.SecondPayment)
                        .With(p => p.DueDate, currentDateTime.AddDays(-22))
                        .With(p => p.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create()
                })
                .With(x => x.EmploymentCheckModels, new List<EmploymentCheckModel>()
                {
                    new EmploymentCheckModel { CheckType = EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck, Result = false },
                    new EmploymentCheckModel { CheckType = EmploymentCheckType.EmployedAt365PaymentDueDateSecondCheck, Result = null }
                })
                .Create();

            _sut = Sut(_sutModel);

            // Act
            _sut.ValidateEmploymentChecks(_mockDateTimeService.Object, _sutModel.PendingPaymentModels.First().Id, _collectionPeriod);

            // Assert
            var validationResult = _sut.PendingPayments.First().PendingPaymentValidationResults.FirstOrDefault(x => x.Step == ValidationStep.EmployedAt365Days);
            validationResult.Should().NotBeNull();
            validationResult.Result.Should().BeFalse();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Then_a_validation_result_is_created_for_the_second_365_day_check(bool employmentCheckResult)
        {
            // Arrange
            var currentDateTime = DateTime.UtcNow;
            _mockDateTimeService.Setup(x => x.UtcNow()).Returns(currentDateTime);

            _sutModel = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .With(x => x.Phase, new IncentivePhase(Phase.Phase3))
                .With(a => a.PendingPaymentModels, new List<PendingPaymentModel>()
                {
                    _fixture.Build<PendingPaymentModel>()
                        .With(x => x.EarningType, EarningType.SecondPayment)
                        .With(p => p.DueDate, currentDateTime.AddDays(-22))
                        .With(p => p.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create()
                })
                .With(x => x.EmploymentCheckModels, new List<EmploymentCheckModel>()
                {
                    new EmploymentCheckModel { CheckType = EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck, Result = false },
                    new EmploymentCheckModel { CheckType = EmploymentCheckType.EmployedAt365PaymentDueDateSecondCheck, Result = employmentCheckResult }
                })
                .Create();

            _sut = Sut(_sutModel);

            // Act
            _sut.ValidateEmploymentChecks(_mockDateTimeService.Object, _sutModel.PendingPaymentModels.First().Id, _collectionPeriod);

            // Assert
            var validationResult = _sut.PendingPayments.First().PendingPaymentValidationResults.FirstOrDefault(x => x.Step == ValidationStep.EmployedAt365Days);
            validationResult.Should().NotBeNull();
            validationResult.Result.Should().Be(employmentCheckResult);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Then_a_validation_result_is_overridden_for_the_second_365_day_check_when_an_non_expired_override_exists(bool employmentCheckResult)
        {
            // Arrange
            var currentDateTime = DateTime.UtcNow;
            _mockDateTimeService.Setup(x => x.UtcNow()).Returns(currentDateTime);

            _sutModel = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .With(x => x.Phase, new IncentivePhase(Phase.Phase3))
                .With(a => a.PendingPaymentModels, new List<PendingPaymentModel>()
                {
                    _fixture.Build<PendingPaymentModel>()
                        .With(x => x.EarningType, EarningType.SecondPayment)
                        .With(p => p.DueDate, currentDateTime.AddDays(-22))
                        .With(p => p.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create()
                })
                .With(x => x.EmploymentCheckModels, new List<EmploymentCheckModel>()
                {
                    new EmploymentCheckModel { CheckType = EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck, Result = false },
                    new EmploymentCheckModel { CheckType = EmploymentCheckType.EmployedAt365PaymentDueDateSecondCheck, Result = employmentCheckResult },
                })
                .Create();

            _sut = Sut(_sutModel);

            _sut.AddValidationOverride(new ValidationOverrideStep(ValidationStep.EmployedAt365Days, DateTime.Now.AddDays(1)), _fixture.Create<ServiceRequest>());

            // Act
            _sut.ValidateEmploymentChecks(_mockDateTimeService.Object, _sutModel.PendingPaymentModels.First().Id, _collectionPeriod);

            // Assert
            var validationResult = _sut.PendingPayments.First().PendingPaymentValidationResults.FirstOrDefault(x => x.Step == ValidationStep.EmployedAt365Days);
            validationResult.Should().NotBeNull();
            validationResult.Result.Should().BeTrue();
        }

        private ApprenticeshipIncentives.ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentives.ApprenticeshipIncentive.Get(model.Id, model);
        }
    }
}
