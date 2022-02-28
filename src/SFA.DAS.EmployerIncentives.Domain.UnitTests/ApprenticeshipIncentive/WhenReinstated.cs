using System;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests
{
    public class WhenReinstated
    {
        private ApprenticeshipIncentives.ApprenticeshipIncentive _sut;
        private ApprenticeshipIncentiveModel _sutModel;
        private Fixture _fixture;
        private CollectionCalendar _collectionCalendar;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sutModel = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .With(x => x.Status, IncentiveStatus.Withdrawn)
                .With(x => x.Phase, new IncentivePhase(Phase.Phase2))
                .With(x => x.StartDate, new DateTime(2021, 08, 01))
                .With(a => a.PendingPaymentModels, new List<PendingPaymentModel>())
                .With(a => a.PaymentModels, new List<PaymentModel>())
                .Create();

            _collectionCalendar = new CollectionCalendar(_fixture.CreateMany<AcademicYear>(), _fixture.CreateMany<CollectionCalendarPeriod>());

            _sut = Sut(_sutModel);
        }

        [Test]        
        public void Then_the_incentive_status_is_updated_when_withdrawn_called_and_no_payments_have_been_made()
        {
            // arrange            

            // act
            _sut.Reinstate(_collectionCalendar);

            // assert            
            _sut.Status.Should().Be(IncentiveStatus.Paused);
            _sut.PausePayments.Should().BeTrue();
            _sut.WithdrawnBy.Should().BeNull();
            _sut.PendingPayments.Count.Should().Be(2);
        }

        private ApprenticeshipIncentives.ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentives.ApprenticeshipIncentive.Get(model.Id, model);
        }
    }
}
