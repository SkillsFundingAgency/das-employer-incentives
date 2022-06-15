using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.Persistence.ApprenticeshipIncentiveDomainRepository
{
    [TestFixture]
    public class WhenFindingAnApprenticeshipIncentiveByPaymentId
    {
        private Commands.Persistence.ApprenticeshipIncentiveDomainRepository _sut;
        private Mock<IApprenticeshipIncentiveDataRepository> _mockApprenticeshipIncentiveDataRepository;
        private Mock<IPaymentDataRepository> _mockPaymentDataRepository;
        private Mock<IApprenticeshipIncentiveFactory> _mockApprenticeshipIncentiveFactory;
        private Mock<IDomainEventDispatcher> _mockDomainEventDispatcher;
        private Guid _paymentId;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize(new ApprenticeshipIncentiveCustomization());
            _paymentId = Guid.NewGuid();

            _mockApprenticeshipIncentiveDataRepository = new Mock<IApprenticeshipIncentiveDataRepository>();
            _mockPaymentDataRepository = new Mock<IPaymentDataRepository>();
            _mockApprenticeshipIncentiveFactory = new Mock<IApprenticeshipIncentiveFactory>();
            _mockDomainEventDispatcher = new Mock<IDomainEventDispatcher>();

            _sut = new Commands.Persistence.ApprenticeshipIncentiveDomainRepository(
                _mockApprenticeshipIncentiveDataRepository.Object,
                _mockPaymentDataRepository.Object,
                _mockApprenticeshipIncentiveFactory.Object,
                _mockDomainEventDispatcher.Object);
        }

        [Test]
        public async Task Then_when_a_match_is_found_on_payment_id_an_apprenticeship_incentive_is_returned()
        {
            // Arrange
            var instance = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();
            var payment = _fixture.Create<PaymentModel>();
            var model = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(x => x.Id, payment.ApprenticeshipIncentiveId)
                .Create();

            _mockPaymentDataRepository
                .Setup(x => x.Get(_paymentId)).ReturnsAsync(payment);

            _mockApprenticeshipIncentiveDataRepository
                .Setup(x => x.Get(payment.ApprenticeshipIncentiveId))
                .ReturnsAsync(model);

            _mockApprenticeshipIncentiveFactory.Setup(x => x.GetExisting(model.Id, model))
                .Returns(instance);

            // Act
            var apprenticeshipIncentive = await _sut.FindByPaymentId(_paymentId);

            // Assert
            apprenticeshipIncentive.Should().Be(instance);
        }

        [Test]
        public void Then_an_exception_is_thrown_when_the_payment_is_not_found()
        {
            // Arrange
            PaymentModel nullPayment = null;

            _mockPaymentDataRepository
                .Setup(x => x.Get(_paymentId)).ReturnsAsync(nullPayment);

            // Act
            Func<Task> action = async () => await _sut.FindByPaymentId(_paymentId);

            // Assert
            action.Should().Throw<ArgumentException>().WithMessage($"Payment id {_paymentId} not found");
        }

        [Test]
        public void Then_an_exception_is_thrown_when_the_incentive_is_not_found()
        {
            // Arrange
            var payment = _fixture.Create<PaymentModel>();
            ApprenticeshipIncentiveModel nullApprenticeship = null;

            _mockPaymentDataRepository
                .Setup(x => x.Get(_paymentId)).ReturnsAsync(payment);

            _mockApprenticeshipIncentiveDataRepository
                .Setup(x => x.Get(payment.ApprenticeshipIncentiveId))
                .ReturnsAsync(nullApprenticeship);

            // Act
            Func<Task> action = async () => await _sut.FindByPaymentId(_paymentId);

            // Assert
            action.Should().Throw<ArgumentException>().WithMessage($"Apprenticeship incentive with ID of {payment.ApprenticeshipIncentiveId} and payment ID of {_paymentId} not found");
        }
    }
}
