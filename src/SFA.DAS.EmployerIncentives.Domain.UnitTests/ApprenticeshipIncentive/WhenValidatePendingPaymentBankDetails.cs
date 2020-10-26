using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests
{
    public class WhenValidatePendingPaymentBankDetails
    {
        private ApprenticeshipIncentive _sut;
        private ApprenticeshipIncentiveModel _sutModel;
        private CollectionPeriod _collectionPeriod;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _collectionPeriod = _fixture.Create<CollectionPeriod>();

            _sutModel = _fixture.Create<ApprenticeshipIncentiveModel>();
            _sut = Sut(_sutModel);
        }

        [Test]
        public void Then_an_exception_is_thrown_when_the_account_passed_in_is_not_valid()
        {
            // arrange            
            var pendingPayment = _sutModel.PendingPaymentModels.First();
            var account = _fixture.Create<Accounts.Account>();
            _sut = Sut(_sutModel);

            // act
            Action result = () => _sut.ValidatePendingPaymentBankDetails(pendingPayment.Id, account, _collectionPeriod);

            // assert
            result.Should().Throw<InvalidPendingPaymentException>().WithMessage($"Unable to validate PendingPayment {pendingPayment.Id} of ApprenticeshipIncentive {_sutModel.Id} because the provided Account record does not match the one against the incentive.");            
        }

        private ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentive.Get(model.Id, model);
        }
    }
}
