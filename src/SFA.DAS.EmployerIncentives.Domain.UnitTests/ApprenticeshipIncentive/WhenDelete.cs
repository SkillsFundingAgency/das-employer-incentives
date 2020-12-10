using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests
{
    public class WhenDelete
    {
        private ApprenticeshipIncentive _sut;
        private ApprenticeshipIncentiveModel _sutModel;
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
                        
            _sut = Sut(_sutModel);
        }

        [Test]        
        public void Then_the_incentive_is_marked_as_deleted_when_delete_called()
        {
            // arrange            

            // act
            _sut.Delete();

            // assert            
            _sut.IsDeleted.Should().BeTrue();
        }

        [Test]
        public void Then_a_DeleteIncentiveException_is_thrown_if_there_are_payment_records_for_the_incentive()
        {
            // arrange            
            _sutModel.PaymentModels = new List<PaymentModel>()
            {
                _fixture.Create<PaymentModel>()
            };

            _sut = Sut(_sutModel);

            // act
            Action action = () => _sut.Delete();

            // assert            
            action
                .Should()
                .Throw<DeleteIncentiveException>()
                .WithMessage("Cannot delete an incentive that has made a Payment");
        }

        private ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentive.Get(model.Id, model);
        }
    }
}
