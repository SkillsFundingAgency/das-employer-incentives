using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.Withdraw;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.Withdraw
{
    public class WhenHandlingWithdrawCommand
    {
        private WithdrawCommandHandler _sut;
        private Mock<IApprenticeshipIncentiveDomainRepository> _mockIncentiveDomainRepository;
        private Mock<ICollectionCalendarService> _mockCollectionCalendarService;
        private Domain.ValueObjects.CollectionPeriod _activePeriod;

        private Fixture _fixture;
     
        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockIncentiveDomainRepository = new Mock<IApprenticeshipIncentiveDomainRepository>();
            _mockCollectionCalendarService = new Mock<ICollectionCalendarService>();

            _activePeriod = new Domain.ValueObjects.CollectionPeriod(2, 2020);
            _activePeriod.SetActive(true);

            var collectionPeriods = new List<Domain.ValueObjects.CollectionPeriod>()
            {
                new Domain.ValueObjects.CollectionPeriod(1, 2020),
                _activePeriod,
                new Domain.ValueObjects.CollectionPeriod(3, 2020)
            };
            _mockCollectionCalendarService.Setup(m => m.Get()).ReturnsAsync(new Domain.ValueObjects.CollectionCalendar(collectionPeriods));

            _fixture.Register(ApprenticeshipIncentiveCreator);

            _sut = new WithdrawCommandHandler(_mockIncentiveDomainRepository.Object, _mockCollectionCalendarService.Object);
        }

        [Test]
        public async Task Then_the_incentive_is_deleted_when_the_incentive_has_no_paid_earnings()
        {
            //Arrange
            var incentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();

            var command = new WithdrawCommand(incentive.Account.Id, incentive.Id);

            _mockIncentiveDomainRepository.Setup(x => x.FindByApprenticeshipId(command.IncentiveApplicationApprenticeshipId)).ReturnsAsync(incentive);

            // Act
            await _sut.Handle(command);

            // Assert
            incentive.IsDeleted.Should().BeTrue();
        }

        [Test]
        public async Task Then_the_incentive_is_not_pausePayments_when_the_incentive_has_paid_earnings()
        {
            //Arrange
            var incentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(p => p.PendingPaymentModels, new List<PendingPaymentModel>())
                .With(p => p.PaymentModels, new List<PaymentModel>())
                .With(p => p.ClawbackPaymentModels, new List<ClawbackPaymentModel>())
                .Create();

            var pendingPaymentModel = _fixture.Build<PendingPaymentModel>()
                .With(p => p.ApprenticeshipIncentiveId, incentiveModel.Id)
                .Create();

            incentiveModel.PendingPaymentModels.Add(pendingPaymentModel);

            var paymentModel = _fixture.Build<PaymentModel>()
                .With(p => p.ApprenticeshipIncentiveId, incentiveModel.Id)
                .With(p => p.PendingPaymentId, pendingPaymentModel.Id)
                .With(p => p.PaidDate, DateTime.Today.AddDays(-1))
                .Create();

            incentiveModel.PaymentModels.Add(paymentModel);

            var incentive = new ApprenticeshipIncentiveFactory().GetExisting(incentiveModel.Id, incentiveModel);

            var command = new WithdrawCommand(incentive.Account.Id, incentive.Id);

            _mockIncentiveDomainRepository.Setup(x => x.FindByApprenticeshipId(command.IncentiveApplicationApprenticeshipId)).ReturnsAsync(incentive);

            // Act
            await _sut.Handle(command);

            // Assert
            incentive.PausePayments.Should().BeFalse();
        }

        [Test]
        public async Task Then_the_incentive_is_not_deleted_when_the_incentive_has_paid_earnings()
        {
            //Arrange
            var incentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(p => p.PendingPaymentModels, new List<PendingPaymentModel>())
                .With(p => p.PaymentModels, new List<PaymentModel>())
                .With(p => p.ClawbackPaymentModels, new List<ClawbackPaymentModel>())
                .Create();

            var pendingPaymentModel = _fixture.Build<PendingPaymentModel>()
                .With(p => p.ApprenticeshipIncentiveId, incentiveModel.Id)
                .Create();

            incentiveModel.PendingPaymentModels.Add(pendingPaymentModel);

            var paymentModel = _fixture.Build<PaymentModel>()
                .With(p => p.ApprenticeshipIncentiveId, incentiveModel.Id)
                .With(p => p.PendingPaymentId, pendingPaymentModel.Id)
                .With(p => p.PaidDate, DateTime.Today.AddDays(-1))
                .Create();

            incentiveModel.PaymentModels.Add(paymentModel);

            var incentive = new ApprenticeshipIncentiveFactory().GetExisting(incentiveModel.Id, incentiveModel);

            var command = new WithdrawCommand(incentive.Account.Id, incentive.Id);

            _mockIncentiveDomainRepository.Setup(x => x.FindByApprenticeshipId(command.IncentiveApplicationApprenticeshipId)).ReturnsAsync(incentive);

            // Act
            await _sut.Handle(command);

            // Assert
            incentive.IsDeleted.Should().BeFalse();
        }

        [Test]
        public async Task Then_the_incentive_status_is_withdrawn_when_the_incentive_has_paid_earnings()
        {
            //Arrange
            var incentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(p => p.PendingPaymentModels, new List<PendingPaymentModel>())
                .With(p => p.PaymentModels, new List<PaymentModel>())
                .With(p => p.ClawbackPaymentModels, new List<ClawbackPaymentModel>())
                .Create();

            var pendingPaymentModel = _fixture.Build<PendingPaymentModel>()
                .With(p => p.ApprenticeshipIncentiveId, incentiveModel.Id)
                .Create();

            incentiveModel.PendingPaymentModels.Add(pendingPaymentModel);

            var paymentModel = _fixture.Build<PaymentModel>()
                .With(p => p.ApprenticeshipIncentiveId, incentiveModel.Id)
                .With(p => p.PendingPaymentId, pendingPaymentModel.Id)
                .With(p => p.PaidDate, DateTime.Today.AddDays(-1))
                .Create();

            incentiveModel.PaymentModels.Add(paymentModel);

            var incentive = new ApprenticeshipIncentiveFactory().GetExisting(incentiveModel.Id, incentiveModel);

            var command = new WithdrawCommand(incentive.Account.Id, incentive.Id);

            _mockIncentiveDomainRepository.Setup(x => x.FindByApprenticeshipId(command.IncentiveApplicationApprenticeshipId)).ReturnsAsync(incentive);

            // Act
            await _sut.Handle(command);

            // Assert
            incentive.GetModel().Status.Should().Be(IncentiveStatus.Withdrawn);
        }

        [Test]
        public async Task Then_the_incentive_unpaid_earnings_are_withdrawn_when_the_incentive_has_paid_earnings()
        {
            //Arrange
            var incentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(p => p.PendingPaymentModels, new List<PendingPaymentModel>())
                .With(p => p.PaymentModels, new List<PaymentModel>())
                .With(p => p.ClawbackPaymentModels, new List<ClawbackPaymentModel>())
                .Create();

            var pendingPaymentModel = _fixture.Build<PendingPaymentModel>()
                .With(p => p.ApprenticeshipIncentiveId, incentiveModel.Id)
                .With(p => p.PaymentMadeDate, (DateTime?)null)
                .Create();

            incentiveModel.PendingPaymentModels.Add(pendingPaymentModel);

            var paymentModel = _fixture.Build<PaymentModel>()
                .With(p => p.ApprenticeshipIncentiveId, incentiveModel.Id)
                .With(p => p.PendingPaymentId, pendingPaymentModel.Id)
                .With(p => p.PaidDate, DateTime.Today.AddDays(-1))
                .Create();

            incentiveModel.PaymentModels.Add(paymentModel);

            var incentive = new ApprenticeshipIncentiveFactory().GetExisting(incentiveModel.Id, incentiveModel);

            var command = new WithdrawCommand(incentive.Account.Id, incentive.Id);

            _mockIncentiveDomainRepository.Setup(x => x.FindByApprenticeshipId(command.IncentiveApplicationApprenticeshipId)).ReturnsAsync(incentive);

            // Act
            await _sut.Handle(command);

            // Assert
            incentive.PendingPayments.Count.Should().Be(0);
        }

        [Test]
        public async Task Then_a_PendingPaymentDeleted_event_is_raised_for_the_incentive_unpaid_earnings_when_the_incentive_has_paid_earnings()
        {
            //Arrange
            var incentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(p => p.PendingPaymentModels, new List<PendingPaymentModel>())
                .With(p => p.PaymentModels, new List<PaymentModel>())
                .With(p => p.ClawbackPaymentModels, new List<ClawbackPaymentModel>())
                .Create();

            var pendingPaymentModel = _fixture.Build<PendingPaymentModel>()
                .With(p => p.ApprenticeshipIncentiveId, incentiveModel.Id)
                .With(p => p.PaymentMadeDate, (DateTime?)null)
                .Create();

            incentiveModel.PendingPaymentModels.Add(pendingPaymentModel);

            var paymentModel = _fixture.Build<PaymentModel>()
                .With(p => p.ApprenticeshipIncentiveId, incentiveModel.Id)
                .With(p => p.PendingPaymentId, pendingPaymentModel.Id)
                .With(p => p.PaidDate, DateTime.Today.AddDays(-1))
                .Create();

            incentiveModel.PaymentModels.Add(paymentModel);

            var incentive = new ApprenticeshipIncentiveFactory().GetExisting(incentiveModel.Id, incentiveModel);

            var command = new WithdrawCommand(incentive.Account.Id, incentive.Id);

            _mockIncentiveDomainRepository.Setup(x => x.FindByApprenticeshipId(command.IncentiveApplicationApprenticeshipId)).ReturnsAsync(incentive);

            // Act
            await _sut.Handle(command);

            // Assert
            var raisedEvent = incentive.FlushEvents().OfType<PendingPaymentDeleted>().Single();
            raisedEvent.AccountId.Should().Be(incentive.Account.Id);
            raisedEvent.AccountLegalEntityId.Should().Be(incentive.Account.AccountLegalEntityId);
            raisedEvent.UniqueLearnerNumber.Should().Be(incentive.Apprenticeship.UniqueLearnerNumber);
            raisedEvent.Model.Should().Be(pendingPaymentModel);
        }

        [Test]
        public async Task Then_a_Clawback_is_added_for_the_payment_when_the_incentive_has_paid_earnings()
        {
            //Arrange
            var incentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(p => p.PendingPaymentModels, new List<PendingPaymentModel>())
                .With(p => p.PaymentModels, new List<PaymentModel>())
                .With(p => p.ClawbackPaymentModels, new List<ClawbackPaymentModel>())
                .Create();

            var pendingPaymentModel = _fixture.Build<PendingPaymentModel>()
                .With(p => p.ApprenticeshipIncentiveId, incentiveModel.Id)
                .Create();

            incentiveModel.PendingPaymentModels.Add(pendingPaymentModel);

            var paymentModel = _fixture.Build<PaymentModel>()
                .With(p => p.ApprenticeshipIncentiveId, incentiveModel.Id)
                .With(p => p.PendingPaymentId, pendingPaymentModel.Id)
                .With(p => p.PaidDate, DateTime.Today.AddDays(-1))
                .Create();

            incentiveModel.PaymentModels.Add(paymentModel);

            var incentive = new ApprenticeshipIncentiveFactory().GetExisting(incentiveModel.Id, incentiveModel);

            var command = new WithdrawCommand(incentive.Account.Id, incentive.Id);

            _mockIncentiveDomainRepository.Setup(x => x.FindByApprenticeshipId(command.IncentiveApplicationApprenticeshipId)).ReturnsAsync(incentive);

            // Act
            await _sut.Handle(command);

            // Assert
            incentive.Clawbacks.Count.Should().Be(1);
            var clawback = incentive.Clawbacks.Single().GetModel();
            clawback.Account.Should().Be(incentive.Account);
            clawback.Amount.Should().Be(pendingPaymentModel.Amount * -1);
            clawback.ApprenticeshipIncentiveId.Should().Be(incentive.Id);
            clawback.PendingPaymentId.Should().Be(pendingPaymentModel.Id);
            clawback.SubnominalCode.Should().Be(paymentModel.SubnominalCode);
            clawback.PaymentId.Should().Be(paymentModel.Id);
            clawback.CreatedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
            clawback.CollectionPeriod.Should().Be(_activePeriod.PeriodNumber);
            clawback.CollectionPeriodYear.Should().Be(_activePeriod.AcademicYear);
        }

        [Test]
        public async Task Then_the_incentive_is_persisted()
        {
            //Arrange
            var incentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();

            var command = new WithdrawCommand(incentive.Account.Id, incentive.Id);

            _mockIncentiveDomainRepository.Setup(x => x.FindByApprenticeshipId(command.IncentiveApplicationApprenticeshipId)).ReturnsAsync(incentive);
                        
            // Act
            await _sut.Handle(command);

            // Assert
            _mockIncentiveDomainRepository
                .Verify(m => m.Save(It.Is<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>(i =>
               i.Id == command.IncentiveApplicationApprenticeshipId)),
                Times.Once);
        }

        [Test]
        public async Task Then_the_incentive_is_not_persisted_if_it_does_not_exist()
        {
            //Arrange
            var incentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();

            var command = new WithdrawCommand(incentive.Account.Id, incentive.Id);

            _mockIncentiveDomainRepository
                .Setup(x => x.FindByApprenticeshipId(
                    command.IncentiveApplicationApprenticeshipId))
                .ReturnsAsync(null as Domain.ApprenticeshipIncentives.ApprenticeshipIncentive);

            // Act
            await _sut.Handle(command);

            // Assert
            _mockIncentiveDomainRepository
                .Verify(m => m.Save(It.IsAny<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>()),
                Times.Never);
        }

        private Domain.ApprenticeshipIncentives.ApprenticeshipIncentive ApprenticeshipIncentiveCreator()
        {
            var incentive = new ApprenticeshipIncentiveFactory()
                .CreateNew(_fixture.Create<Guid>(),
                    _fixture.Create<Guid>(),
                    _fixture.Create<Account>(),
                    new Apprenticeship(
                        _fixture.Create<long>(),
                        _fixture.Create<string>(),
                        _fixture.Create<string>(),
                        DateTime.Today.AddYears(-26),
                        _fixture.Create<long>(),
                        ApprenticeshipEmployerType.Levy,
                        _fixture.Create<string>(),
                        _fixture.Create<DateTime>()
                    ),
                    DateTime.Today,
                    _fixture.Create<DateTime>(),
                    _fixture.Create<string>(),
                    _fixture.Create<int>());

            return incentive;
        }
    }
}
