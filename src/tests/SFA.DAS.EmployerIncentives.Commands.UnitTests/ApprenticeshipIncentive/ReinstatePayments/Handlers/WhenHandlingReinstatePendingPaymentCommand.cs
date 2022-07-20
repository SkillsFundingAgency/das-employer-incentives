using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.ReinstatePayments;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.ReinstatePayments.Handlers
{
    [TestFixture]
    public class WhenHandlingReinstatePendingPaymentCommand
    {
        private Fixture _fixture;
        private ReinstatePendingPaymentCommandHandler _sut;
        private Mock<IApprenticeshipIncentiveArchiveRepository> _archiveRepository;
        private Mock<IApprenticeshipIncentiveDomainRepository> _domainRepository;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _archiveRepository = new Mock<IApprenticeshipIncentiveArchiveRepository>();
            _domainRepository = new Mock<IApprenticeshipIncentiveDomainRepository>();
            _sut = new ReinstatePendingPaymentCommandHandler(_archiveRepository.Object, _domainRepository.Object);
        }

        [Test]
        public async Task Then_the_pending_payment_is_reinstated_for_the_specified_id()
        {
            // Arrange
            var archivedPendingPayment = _fixture.Create<PendingPaymentModel>();

            _archiveRepository.Setup(x => x.Get(archivedPendingPayment.Id)).ReturnsAsync(archivedPendingPayment);

            var incentive = Domain.ApprenticeshipIncentives.ApprenticeshipIncentive.Get(archivedPendingPayment.ApprenticeshipIncentiveId,
                _fixture.Build<ApprenticeshipIncentiveModel>()
                    .With(x => x.PendingPaymentModels, new List<PendingPaymentModel>())
                    .Create());

            _domainRepository.Setup(x => x.Find(archivedPendingPayment.ApprenticeshipIncentiveId)).ReturnsAsync(incentive);

            var command = new ReinstatePendingPaymentCommand(archivedPendingPayment.Id, _fixture.Create<ReinstatePaymentRequest>());

            // Act
            await _sut.Handle(command);

            // Assert
            _domainRepository.Verify(x => x.Save(It.Is<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>(
                     y => y.PendingPayments.Count == 1
                     && !y.PendingPayments.ToList()[0].PaymentMadeDate.HasValue
                     && y.PendingPayments.ToList()[0].Id == archivedPendingPayment.Id
                     && y.PendingPayments.ToList()[0].Amount == archivedPendingPayment.Amount
                     && y.PendingPayments.ToList()[0].EarningType == archivedPendingPayment.EarningType
                     )), Times.Once);
        }

        [Test]
        public void Then_an_exception_is_thrown_if_the_pending_payment_cannot_be_found()
        {
            // Arrange
            PendingPaymentModel nullPendingPaymentModel = null;
            _archiveRepository.Setup(x => x.Get(It.IsAny<Guid>())).ReturnsAsync(nullPendingPaymentModel);

            var command = _fixture.Create<ReinstatePendingPaymentCommand>();

            // Act
            Func<Task> action = async () => await _sut.Handle(command);

            // Assert
            action.Should().Throw<ArgumentException>().WithMessage($"Pending payment with ID {command.PendingPaymentId} not found");
        }

        [Test]
        public void Then_an_exception_is_thrown_if_the_incentive_cannot_be_found()
        {
            // Arrange
            var archivedPendingPayment = _fixture.Create<PendingPaymentModel>();

            _archiveRepository.Setup(x => x.Get(archivedPendingPayment.Id)).ReturnsAsync(archivedPendingPayment);

            Domain.ApprenticeshipIncentives.ApprenticeshipIncentive nullIncentive = null;

            _domainRepository.Setup(x => x.Find(archivedPendingPayment.ApprenticeshipIncentiveId)).ReturnsAsync(nullIncentive);

            var command = new ReinstatePendingPaymentCommand(archivedPendingPayment.Id, _fixture.Create<ReinstatePaymentRequest>());

            // Act
            Func<Task> action = async () => await _sut.Handle(command);

            // Assert
            action.Should().Throw<ArgumentException>().WithMessage($"Apprenticeship incentive with ID {archivedPendingPayment.ApprenticeshipIncentiveId} for pending payment ID {archivedPendingPayment.Id} not found");
        }
    }
}
