using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.UnitTests.ApprenticeshipIncentives
{
    public class WhenClawbackAddedEventIsHandled
    {
        private ClawBackAddedHandler _sut;
        private Mock<IClawbackDataRepository> _mockClawbackDataRepository;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockClawbackDataRepository = new Mock<IClawbackDataRepository>();

            _sut = new ClawBackAddedHandler(_mockClawbackDataRepository.Object);
        }

        [Test]
        public async Task Then_a_clawback_payment_is_persisted()
        {
            //Arrange
            var apprenticeshipIncentiveId = Guid.NewGuid();

            var pendingPaymentModels = _fixture.Build<PendingPaymentModel>()
                .With(m => m.ApprenticeshipIncentiveId, apprenticeshipIncentiveId)
                .With(m => m.ClawedBack, false)
                .CreateMany(2);

            var paymentModels = _fixture.Build<PaymentModel>()
                .With(m => m.ApprenticeshipIncentiveId, apprenticeshipIncentiveId)
                .CreateMany(2);

            pendingPaymentModels.First().ClawedBack = true;
            paymentModels.First().PendingPaymentId = pendingPaymentModels.First().Id;

            var model = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(m => m.ApplicationApprenticeshipId, apprenticeshipIncentiveId)
                .With(m => m.PendingPaymentModels, new List<PendingPaymentModel>(pendingPaymentModels))
                .With(m => m.PaymentModels, new List<PaymentModel>(paymentModels))
                .With(m => m.Account, pendingPaymentModels.First().Account)
                .Create();

            var @event = new ClawBackAdded(model);

            //Act
            await _sut.Handle(@event);

            //Assert
            _mockClawbackDataRepository.Verify(m =>
            m.Add(It.Is<Clawback>(i =>
                   i.ApprenticeshipIncentiveId == @event.Model.ApplicationApprenticeshipId &&
                   i.PendingPaymentId == @event.Model.PendingPaymentModels.First().Id &&
                   i.Account == @event.Model.Account &&
                   i.Amount == @event.Model.PendingPaymentModels.First().Amount &&
                   i.SubnominalCode == @event.Model.PaymentModels.First().SubnominalCode &&
                   i.PaymentId == @event.Model.PaymentModels.First().Id
                   )),
                   Times.Once);
        }
    }
}
