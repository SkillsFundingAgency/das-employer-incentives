using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests
{
    public class WhenWithdrawn
    {
        private ApprenticeshipIncentives.ApprenticeshipIncentive _sut;
        private ApprenticeshipIncentiveModel _sutModel;
        private CollectionCalendar _collectionCalendar;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _fixture.Build<PendingPaymentModel>().With(p => p.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create();

            _sutModel = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .With(a => a.PendingPaymentModels, new List<PendingPaymentModel>() 
                    {
                        _fixture.Build<PendingPaymentModel>().With(p => p.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>()).Create()
                    })
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
            _sut.Withdraw(WithdrawnBy.Compliance, _collectionCalendar);

            // assert            
            _sut.Status.Should().Be(IncentiveStatus.Withdrawn);
            _sut.WithdrawnBy.Should().Be(WithdrawnBy.Compliance);
            _sut.IsDeleted.Should().BeFalse();
        }

        private ApprenticeshipIncentives.ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentives.ApprenticeshipIncentive.Get(model.Id, model);
        }
    }
}
