﻿using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.IncentiveApplication;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Events.UnitTests.ApprenticeshipIncentives
{
    public class WhenPaymentsResumedEventIsHandled
    {
        private PaymentsResumedHandler _sut;
        private Mock<IIncentiveApplicationStatusAuditDataRepository> _mockAuditDataRepository;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockAuditDataRepository = new Mock<IIncentiveApplicationStatusAuditDataRepository>();

            _sut = new PaymentsResumedHandler(_mockAuditDataRepository.Object);
        }

        [Test]
        public async Task Then_an_audit_is_persisted()
        {
            //Arrange
            var apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .Without(x => x.BreakInLearnings)
                .Create();

            var @event = new PaymentsResumed(apprenticeshipIncentiveModel.Account.Id,
                apprenticeshipIncentiveModel.Account.AccountLegalEntityId,
                apprenticeshipIncentiveModel,
                _fixture.Create<ServiceRequest>());

            //Act
            await _sut.Handle(@event);

            //Assert
            _mockAuditDataRepository.Verify(m =>
            m.Add(It.Is<IncentiveApplicationAudit>(i =>
                   i.IncentiveApplicationApprenticeshipId == @event.Model.ApplicationApprenticeshipId &&
                   i.Process == Enums.IncentiveApplicationStatus.PaymentsResumed &&
                   i.ServiceRequest == @event.ServiceRequest)),
                   Times.Once);
        }
    }
}
