using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests
{
    public class WhenWithdrawn
    {
        private ApprenticeshipIncentive _sut;
        private ApprenticeshipIncentiveModel _sutModel;
        private Mock<ICollectionCalendarService> _mockCollectionCalendarService;
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

            _mockCollectionCalendarService = new Mock<ICollectionCalendarService>();

            _sut = Sut(_sutModel);
        }

        [Test]        
        public void Then_the_incentive_is_marked_as_deleted_when_withdrawn_called_and_no_payments_have_been_made()
        {
            // arrange            

            // act
            _sut.Withdraw(_mockCollectionCalendarService.Object);

            // assert            
            _sut.IsDeleted.Should().BeTrue();
        }

        private ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentive.Get(model.Id, model);
        }
    }
}
