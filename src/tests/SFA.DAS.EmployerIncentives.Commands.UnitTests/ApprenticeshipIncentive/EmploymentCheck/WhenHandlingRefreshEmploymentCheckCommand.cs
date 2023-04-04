using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.EmploymentCheck;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.EmploymentCheck
{
    [TestFixture]
    public class WhenHandlingRefreshEmploymentCheckCommand
    {
        private RefreshEmploymentCheckCommandHandler _sut;
        private Mock<IApprenticeshipIncentiveDomainRepository> _mockIncentiveDomainRespository;
        private Mock<IDateTimeService> _mockDateTimeService;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockDateTimeService = new Mock<IDateTimeService>();
            _mockDateTimeService.Setup(m => m.Now()).Returns(DateTime.Now);
            _mockDateTimeService.Setup(m => m.UtcNow()).Returns(DateTime.UtcNow);


            _mockIncentiveDomainRespository = new Mock<IApprenticeshipIncentiveDomainRepository>();

            _sut = new RefreshEmploymentCheckCommandHandler(_mockIncentiveDomainRespository.Object,
                _mockDateTimeService.Object);
        }

        [Test]
        public async Task Then_a_refresh_of_the_initial_employment_checks_are_requested_if_the_apprentices_have_a_learning_record()
        {
            // Arrange
            var model = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(x => x.EmploymentCheckModels, _fixture.CreateMany<EmploymentCheckModel>(2).ToList())
                .With(x => x.StartDate, new DateTime(2021, 10, 01))
                .With(x => x.Phase, new IncentivePhase(Phase.Phase2))
                .Create();

            var serviceRequest = _fixture.Create<ServiceRequest>();

            var incentive = Domain.ApprenticeshipIncentives.ApprenticeshipIncentive.Get(model.Id, model);

            _mockIncentiveDomainRespository
                .Setup(x => x.FindByUlnWithinAccountLegalEntity(
                    model.Apprenticeship.UniqueLearnerNumber,
                    model.Account.AccountLegalEntityId))
                .ReturnsAsync(incentive);

            // Act
            await _sut.Handle(new RefreshEmploymentCheckCommand(
                RefreshEmploymentCheckType.InitialEmploymentChecks.ToString(),
                model.Account.AccountLegalEntityId,
                model.Apprenticeship.UniqueLearnerNumber,
                serviceRequest.TaskId,
                serviceRequest.DecisionReference,
                serviceRequest.Created
            ));

            // Assert
            var createdEvents = incentive.FlushEvents().OfType<EmploymentChecksCreated>();
            createdEvents.Should().NotBeNull();
            createdEvents.Count().Should().Be(2);
            _ = createdEvents.First().ApprenticeshipIncentiveId.Should().Be(model.Id);
            _ = createdEvents.First().ServiceRequest.TaskId.Should().Be(serviceRequest.TaskId);
            _ = createdEvents.First().ServiceRequest.DecisionReference.Should().Be(serviceRequest.DecisionReference);
            _ = createdEvents.First().ServiceRequest.Created.Should().Be(serviceRequest.Created);

            _ = createdEvents.Last().ApprenticeshipIncentiveId.Should().Be(model.Id);
            _ = createdEvents.Last().ServiceRequest.TaskId.Should().Be(serviceRequest.TaskId);
            _ = createdEvents.Last().ServiceRequest.DecisionReference.Should().Be(serviceRequest.DecisionReference);
            _ = createdEvents.Last().ServiceRequest.Created.Should().Be(serviceRequest.Created);

            _mockIncentiveDomainRespository.Verify(m => m.Save(incentive), Times.Once);
        }

        [Test]
        public void Then_a_refresh_of_the_employment_check_is_not_requested_if_the_apprentice_does_not_exist()
        {
            var model = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(x => x.EmploymentCheckModels, _fixture.CreateMany<EmploymentCheckModel>(2).ToList())
                .With(x => x.StartDate, new DateTime(2021, 10, 01))
                .With(x => x.Phase, new IncentivePhase(Phase.Phase2))
                .Create();

            var serviceRequest = _fixture.Create<ServiceRequest>();

            var incentive = Domain.ApprenticeshipIncentives.ApprenticeshipIncentive.Get(model.Id, model);

            _mockIncentiveDomainRespository
                .Setup(x => x.FindByUlnWithinAccountLegalEntity(
                    model.Apprenticeship.UniqueLearnerNumber,
                    model.Account.AccountLegalEntityId))
                .Returns(Task.FromResult<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>(null));

            // Act
            Func<Task> result = async () => await _sut.Handle(new RefreshEmploymentCheckCommand(
                RefreshEmploymentCheckType.InitialEmploymentChecks.ToString(),
                model.Account.AccountLegalEntityId,
                model.Apprenticeship.UniqueLearnerNumber,
                serviceRequest.TaskId,
                serviceRequest.DecisionReference,
                serviceRequest.Created
            ));

            // Assert
            result.Should().ThrowAsync<ArgumentException>().WithMessage(
                $"Apprenticeship incentive with account legal entity of {model.Account.AccountLegalEntityId} and ULN {model.Apprenticeship.UniqueLearnerNumber} not found");

            var createdEvent = incentive.FlushEvents().SingleOrDefault() as EmploymentChecksCreated;
            createdEvent.Should().BeNull();

            _mockIncentiveDomainRespository.Verify(m => m.Save(incentive), Times.Never);
        }

        [Test]
        public void Then_a_refresh_of_the_365_day_employment_check_is_not_requested_if_the_initial_checks_have_not_been_completed()
        {
            // Arrange
            var model = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(x => x.EmploymentCheckModels, new List<EmploymentCheckModel>())
                .With(x => x.StartDate, new DateTime(2021, 10, 01))
                .With(x => x.Phase, new IncentivePhase(Phase.Phase2))
                .Create();

            var serviceRequest = _fixture.Create<ServiceRequest>();

            var incentive = Domain.ApprenticeshipIncentives.ApprenticeshipIncentive.Get(model.Id, model);

            _mockIncentiveDomainRespository
                .Setup(x => x.FindByUlnWithinAccountLegalEntity(
                    model.Apprenticeship.UniqueLearnerNumber,
                    model.Account.AccountLegalEntityId))
                .ReturnsAsync(incentive);

            // Act
            Func<Task> result = async () => await _sut.Handle(new RefreshEmploymentCheckCommand(
                RefreshEmploymentCheckType.EmployedAt365DaysCheck.ToString(),
                model.Account.AccountLegalEntityId,
                model.Apprenticeship.UniqueLearnerNumber,
                serviceRequest.TaskId,
                serviceRequest.DecisionReference,
                serviceRequest.Created
            ));

            // Assert
            result.Should().ThrowAsync<InvalidOperationException>().WithMessage("Employed at 365 days check cannot be refreshed if initial employment checks have not completed");
            
            _mockIncentiveDomainRespository.Verify(m => m.Save(incentive), Times.Never);
        }

        [TestCase(false, false)]
        [TestCase(true, true)]
        [TestCase(false, true)]
        public void Then_a_refresh_of_the_365_day_employment_check_is_not_requested_if_the_initial_checks_have_not_passed(bool firstCheckResult, bool secondCheckResult)
        {
            // Arrange
            var model = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(x => x.EmploymentCheckModels, new List<EmploymentCheckModel>
                {
                    _fixture.Build<EmploymentCheckModel>()
                        .With(x => x.CheckType, EmploymentCheckType.EmployedAtStartOfApprenticeship)
                        .With(x => x.Result, firstCheckResult)
                        .Create(),
                    _fixture.Build<EmploymentCheckModel>()
                        .With(x => x.CheckType, EmploymentCheckType.EmployedBeforeSchemeStarted)
                        .With(x => x.Result, secondCheckResult)
                        .Create()
                })
                .With(x => x.StartDate, new DateTime(2021, 10, 01))
                .With(x => x.Phase, new IncentivePhase(Phase.Phase2))
                .Create();

            var serviceRequest = _fixture.Create<ServiceRequest>();

            var incentive = Domain.ApprenticeshipIncentives.ApprenticeshipIncentive.Get(model.Id, model);

            _mockIncentiveDomainRespository
                .Setup(x => x.FindByUlnWithinAccountLegalEntity(
                    model.Apprenticeship.UniqueLearnerNumber,
                    model.Account.AccountLegalEntityId))
                .ReturnsAsync(incentive);

            // Act
            Func<Task> result = async () => await _sut.Handle(new RefreshEmploymentCheckCommand(
                RefreshEmploymentCheckType.EmployedAt365DaysCheck.ToString(),
                model.Account.AccountLegalEntityId,
                model.Apprenticeship.UniqueLearnerNumber,
                serviceRequest.TaskId,
                serviceRequest.DecisionReference,
                serviceRequest.Created
            ));

            // Assert
            result.Should().ThrowAsync<InvalidOperationException>().WithMessage("Employed at 365 days check cannot be refreshed if initial employment checks have not completed");

            _mockIncentiveDomainRespository.Verify(m => m.Save(incentive), Times.Never);
        }

        [Test]
        public void Then_a_refresh_of_the_365_day_employment_check_is_not_requested_if_the_365_day_checks_have_not_been_executed()
        {
            // Arrange
            var model = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(x => x.EmploymentCheckModels, new List<EmploymentCheckModel>
                {
                    _fixture.Build<EmploymentCheckModel>()
                        .With(x => x.CheckType, EmploymentCheckType.EmployedAtStartOfApprenticeship)
                        .With(x => x.Result, true)
                        .Create(),
                    _fixture.Build<EmploymentCheckModel>()
                        .With(x => x.CheckType, EmploymentCheckType.EmployedBeforeSchemeStarted)
                        .With(x => x.Result, false)
                        .Create()
                })
                .With(x => x.StartDate, new DateTime(2021, 10, 01))
                .With(x => x.Phase, new IncentivePhase(Phase.Phase2))
                .Create();

            var serviceRequest = _fixture.Create<ServiceRequest>();

            var incentive = Domain.ApprenticeshipIncentives.ApprenticeshipIncentive.Get(model.Id, model);

            _mockIncentiveDomainRespository
                .Setup(x => x.FindByUlnWithinAccountLegalEntity(
                    model.Apprenticeship.UniqueLearnerNumber,
                    model.Account.AccountLegalEntityId))
                .ReturnsAsync(incentive);

            // Act
            Func<Task> result = async () => await _sut.Handle(new RefreshEmploymentCheckCommand(
                RefreshEmploymentCheckType.EmployedAt365DaysCheck.ToString(),
                model.Account.AccountLegalEntityId,
                model.Apprenticeship.UniqueLearnerNumber,
                serviceRequest.TaskId,
                serviceRequest.DecisionReference,
                serviceRequest.Created
            ));

            // Assert
            result.Should().ThrowAsync<InvalidOperationException>().WithMessage("Employed at 365 days check cannot be refreshed if 365 day employment checks have not previously executed");

            _mockIncentiveDomainRespository.Verify(m => m.Save(incentive), Times.Never);
        }

        [Test]
        public void Then_a_refresh_of_the_365_day_employment_check_is_not_requested_if_the_second_365_day_check_has_not_been_executed()
        {
            // Arrange
            var model = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(x => x.EmploymentCheckModels, new List<EmploymentCheckModel>
                {
                    _fixture.Build<EmploymentCheckModel>()
                        .With(x => x.CheckType, EmploymentCheckType.EmployedAtStartOfApprenticeship)
                        .With(x => x.Result, true)
                        .Create(),
                    _fixture.Build<EmploymentCheckModel>()
                        .With(x => x.CheckType, EmploymentCheckType.EmployedBeforeSchemeStarted)
                        .With(x => x.Result, false)
                        .Create(),
                    _fixture.Build<EmploymentCheckModel>()
                        .With(x => x.CheckType, EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck)
                        .With(x => x.Result, false)
                        .Create()
                })
                .With(x => x.StartDate, new DateTime(2021, 10, 01))
                .With(x => x.Phase, new IncentivePhase(Phase.Phase2))
                .Create();

            var serviceRequest = _fixture.Create<ServiceRequest>();

            var incentive = Domain.ApprenticeshipIncentives.ApprenticeshipIncentive.Get(model.Id, model);

            _mockIncentiveDomainRespository
                .Setup(x => x.FindByUlnWithinAccountLegalEntity(
                    model.Apprenticeship.UniqueLearnerNumber,
                    model.Account.AccountLegalEntityId))
                .ReturnsAsync(incentive);

            // Act
            Func<Task> result = async () => await _sut.Handle(new RefreshEmploymentCheckCommand(
                RefreshEmploymentCheckType.EmployedAt365DaysCheck.ToString(),
                model.Account.AccountLegalEntityId,
                model.Apprenticeship.UniqueLearnerNumber,
                serviceRequest.TaskId,
                serviceRequest.DecisionReference,
                serviceRequest.Created
            ));

            // Assert
            result.Should().ThrowAsync<InvalidOperationException>().WithMessage("Employed at 365 days check cannot be refreshed if 365 day employment checks have not previously executed");

            _mockIncentiveDomainRespository.Verify(m => m.Save(incentive), Times.Never);
        }

        [Test]
        public void Then_a_refresh_of_the_365_day_employment_check_is_not_requested_if_the_first_365_day_check_has_not_returned_a_result()
        {
            // Arrange
            var model = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(x => x.EmploymentCheckModels, new List<EmploymentCheckModel>
                {
                    _fixture.Build<EmploymentCheckModel>()
                        .With(x => x.CheckType, EmploymentCheckType.EmployedAtStartOfApprenticeship)
                        .With(x => x.Result, true)
                        .Create(),
                    _fixture.Build<EmploymentCheckModel>()
                        .With(x => x.CheckType, EmploymentCheckType.EmployedBeforeSchemeStarted)
                        .With(x => x.Result, false)
                        .Create(),
                    _fixture.Build<EmploymentCheckModel>()
                        .With(x => x.CheckType, EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck)
                        .Without(x => x.Result)
                        .Create()
                })
                .With(x => x.StartDate, new DateTime(2021, 10, 01))
                .With(x => x.Phase, new IncentivePhase(Phase.Phase2))
                .Create();

            var serviceRequest = _fixture.Create<ServiceRequest>();

            var incentive = Domain.ApprenticeshipIncentives.ApprenticeshipIncentive.Get(model.Id, model);

            _mockIncentiveDomainRespository
                .Setup(x => x.FindByUlnWithinAccountLegalEntity(
                    model.Apprenticeship.UniqueLearnerNumber,
                    model.Account.AccountLegalEntityId))
                .ReturnsAsync(incentive);

            // Act
            Func<Task> result = async () => await _sut.Handle(new RefreshEmploymentCheckCommand(
                RefreshEmploymentCheckType.EmployedAt365DaysCheck.ToString(),
                model.Account.AccountLegalEntityId,
                model.Apprenticeship.UniqueLearnerNumber,
                serviceRequest.TaskId,
                serviceRequest.DecisionReference,
                serviceRequest.Created
            ));

            // Assert
            result.Should().ThrowAsync<InvalidOperationException>().WithMessage("Employed at 365 days check cannot be refreshed if 365 day employment checks have not previously executed");

            _mockIncentiveDomainRespository.Verify(m => m.Save(incentive), Times.Never);
        }

        [Test]
        public void Then_a_refresh_of_the_365_day_employment_check_is_not_requested_if_the_second_365_day_check_has_not_returned_a_result()
        {
            // Arrange
            var model = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(x => x.EmploymentCheckModels, new List<EmploymentCheckModel>
                {
                    _fixture.Build<EmploymentCheckModel>()
                        .With(x => x.CheckType, EmploymentCheckType.EmployedAtStartOfApprenticeship)
                        .With(x => x.Result, true)
                        .Create(),
                    _fixture.Build<EmploymentCheckModel>()
                        .With(x => x.CheckType, EmploymentCheckType.EmployedBeforeSchemeStarted)
                        .With(x => x.Result, false)
                        .Create(),
                    _fixture.Build<EmploymentCheckModel>()
                        .With(x => x.CheckType, EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck)
                        .Without(x => x.Result)
                        .Without(x => x.ErrorType)
                        .Create(),
                    _fixture.Build<EmploymentCheckModel>()
                        .With(x => x.CheckType, EmploymentCheckType.EmployedAt365PaymentDueDateSecondCheck)
                        .Without(x => x.Result)
                        .Without(x => x.ErrorType)
                        .Create()
                })
                .With(x => x.StartDate, new DateTime(2021, 10, 01))
                .With(x => x.Phase, new IncentivePhase(Phase.Phase2))
                .Create();

            var serviceRequest = _fixture.Create<ServiceRequest>();

            var incentive = Domain.ApprenticeshipIncentives.ApprenticeshipIncentive.Get(model.Id, model);

            _mockIncentiveDomainRespository
                .Setup(x => x.FindByUlnWithinAccountLegalEntity(
                    model.Apprenticeship.UniqueLearnerNumber,
                    model.Account.AccountLegalEntityId))
                .ReturnsAsync(incentive);

            // Act
            Func<Task> result = async () => await _sut.Handle(new RefreshEmploymentCheckCommand(
                RefreshEmploymentCheckType.EmployedAt365DaysCheck.ToString(),
                model.Account.AccountLegalEntityId,
                model.Apprenticeship.UniqueLearnerNumber,
                serviceRequest.TaskId,
                serviceRequest.DecisionReference,
                serviceRequest.Created
            ));

            // Assert
            result.Should().ThrowAsync<InvalidOperationException>().WithMessage("Employed at 365 days check cannot be refreshed if 365 day employment checks have not previously executed");

            _mockIncentiveDomainRespository.Verify(m => m.Save(incentive), Times.Never);
        }

        [Test]
        public async Task Then_a_refresh_of_the_365_day_employment_checks_are_requested_if_initial_and_365_day_checks_have_been_executed()
        {
            // Arrange
            var model = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(x => x.EmploymentCheckModels, new List<EmploymentCheckModel>
                {
                    _fixture.Build<EmploymentCheckModel>()
                        .With(x => x.CheckType, EmploymentCheckType.EmployedAtStartOfApprenticeship)
                        .With(x => x.Result, true)
                        .Create(),
                    _fixture.Build<EmploymentCheckModel>()
                        .With(x => x.CheckType, EmploymentCheckType.EmployedBeforeSchemeStarted)
                        .With(x => x.Result, false)
                        .Create(),
                    _fixture.Build<EmploymentCheckModel>()
                        .With(x => x.CheckType, EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck)
                        .With(x => x.Result, false)
                        .Create(),
                    _fixture.Build<EmploymentCheckModel>()
                        .With(x => x.CheckType, EmploymentCheckType.EmployedAt365PaymentDueDateSecondCheck)
                        .With(x => x.Result, false)
                        .Create()
                })
                .With(x => x.StartDate, new DateTime(2020, 10, 01))
                .With(x => x.Phase, new IncentivePhase(Phase.Phase1))
                .With(x => x.PendingPaymentModels, new List<PendingPaymentModel>
                {
                    _fixture.Build<PendingPaymentModel>()
                        .With(x => x.EarningType, EarningType.FirstPayment)
                        .With(x => x.DueDate, new DateTime(2020, 10, 01).AddDays(90))
                        .Create(),
                    _fixture.Build<PendingPaymentModel>()
                        .With(x => x.EarningType, EarningType.SecondPayment)
                        .With(x => x.DueDate, new DateTime(2020, 10, 01).AddDays(365))
                        .Create()
                })
                .Create();

            var serviceRequest = _fixture.Create<ServiceRequest>();

            var incentive = Domain.ApprenticeshipIncentives.ApprenticeshipIncentive.Get(model.Id, model);

            _mockIncentiveDomainRespository
                .Setup(x => x.FindByUlnWithinAccountLegalEntity(
                    model.Apprenticeship.UniqueLearnerNumber,
                    model.Account.AccountLegalEntityId))
                .ReturnsAsync(incentive);

            // Act
            await _sut.Handle(new RefreshEmploymentCheckCommand(
                RefreshEmploymentCheckType.EmployedAt365DaysCheck.ToString(),
                model.Account.AccountLegalEntityId,
                model.Apprenticeship.UniqueLearnerNumber,
                serviceRequest.TaskId,
                serviceRequest.DecisionReference,
                serviceRequest.Created
            ));

            // Assert
            var createdEvents = incentive.FlushEvents().OfType<EmploymentChecksCreated>();
            createdEvents.Should().NotBeNull();
            createdEvents.Count().Should().Be(1);
            var checkCreatedEvent = createdEvents.First();
            checkCreatedEvent.ApprenticeshipIncentiveId.Should().Be(model.Id);
            checkCreatedEvent.Model.CheckType.Should().Be(EmploymentCheckType.EmployedAt365PaymentDueDateSecondCheck);
            checkCreatedEvent.ServiceRequest.TaskId.Should().Be(serviceRequest.TaskId);
            checkCreatedEvent.ServiceRequest.DecisionReference.Should().Be(serviceRequest.DecisionReference);
            checkCreatedEvent.ServiceRequest.Created.Should().Be(serviceRequest.Created);
            
            _mockIncentiveDomainRespository.Verify(m => m.Save(incentive), Times.Once);
        }

        [Test]
        public async Task Then_a_refresh_of_the_365_day_employment_checks_are_requested_if_the_checks_have_returned_an_error()
        {
            // Arrange
            var model = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(x => x.EmploymentCheckModels, new List<EmploymentCheckModel>
                {
                    _fixture.Build<EmploymentCheckModel>()
                        .With(x => x.CheckType, EmploymentCheckType.EmployedAtStartOfApprenticeship)
                        .With(x => x.Result, true)
                        .Create(),
                    _fixture.Build<EmploymentCheckModel>()
                        .With(x => x.CheckType, EmploymentCheckType.EmployedBeforeSchemeStarted)
                        .With(x => x.Result, false)
                        .Create(),
                    _fixture.Build<EmploymentCheckModel>()
                        .With(x => x.CheckType, EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck)
                        .Without(x => x.Result)
                        .With(x => x.ErrorType, EmploymentCheckResultError.PAYENotFound)
                        .Create(),
                    _fixture.Build<EmploymentCheckModel>()
                        .With(x => x.CheckType, EmploymentCheckType.EmployedAt365PaymentDueDateSecondCheck)
                        .Without(x => x.Result)
                        .With(x => x.ErrorType, EmploymentCheckResultError.NinoAndPAYENotFound)
                        .Create()
                })
                .With(x => x.StartDate, new DateTime(2020, 10, 01))
                .With(x => x.Phase, new IncentivePhase(Phase.Phase1))
                .With(x => x.PendingPaymentModels, new List<PendingPaymentModel>
                {
                    _fixture.Build<PendingPaymentModel>()
                        .With(x => x.EarningType, EarningType.FirstPayment)
                        .With(x => x.DueDate, new DateTime(2020, 10, 01).AddDays(90))
                        .Create(),
                    _fixture.Build<PendingPaymentModel>()
                        .With(x => x.EarningType, EarningType.SecondPayment)
                        .With(x => x.DueDate, new DateTime(2020, 10, 01).AddDays(365))
                        .Create()
                })
                .Create();

            var serviceRequest = _fixture.Create<ServiceRequest>();

            var incentive = Domain.ApprenticeshipIncentives.ApprenticeshipIncentive.Get(model.Id, model);

            _mockIncentiveDomainRespository
                .Setup(x => x.FindByUlnWithinAccountLegalEntity(
                    model.Apprenticeship.UniqueLearnerNumber,
                    model.Account.AccountLegalEntityId))
                .ReturnsAsync(incentive);

            // Act
            await _sut.Handle(new RefreshEmploymentCheckCommand(
                RefreshEmploymentCheckType.EmployedAt365DaysCheck.ToString(),
                model.Account.AccountLegalEntityId,
                model.Apprenticeship.UniqueLearnerNumber,
                serviceRequest.TaskId,
                serviceRequest.DecisionReference,
                serviceRequest.Created
            ));

            // Assert
            var createdEvents = incentive.FlushEvents().OfType<EmploymentChecksCreated>();
            createdEvents.Should().NotBeNull();
            createdEvents.Count().Should().Be(1);
            var checkCreatedEvent = createdEvents.First();
            checkCreatedEvent.ApprenticeshipIncentiveId.Should().Be(model.Id);
            checkCreatedEvent.Model.CheckType.Should().Be(EmploymentCheckType.EmployedAt365PaymentDueDateSecondCheck);
            checkCreatedEvent.ServiceRequest.TaskId.Should().Be(serviceRequest.TaskId);
            checkCreatedEvent.ServiceRequest.DecisionReference.Should().Be(serviceRequest.DecisionReference);
            checkCreatedEvent.ServiceRequest.Created.Should().Be(serviceRequest.Created);

            _mockIncentiveDomainRespository.Verify(m => m.Save(incentive), Times.Once);
        }
    }
}
