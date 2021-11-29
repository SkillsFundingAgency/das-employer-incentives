using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests
{
    public class WhenValidateMinimumRequiredAgreementVersion
    {
        private ApprenticeshipIncentives.ApprenticeshipIncentive _sut;
        private ApprenticeshipIncentiveModel _sutModel;
        private CollectionPeriod _collectionPeriod;
        private Accounts.Account _account;
        private short _collectionYear;
        private long _accountLegalEntityId;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _collectionYear = _fixture.Create<short>();

            
            _collectionPeriod = new CollectionPeriod(1, _collectionYear);

            _accountLegalEntityId = _fixture.Create<long>();
            _account = Accounts.Account.Create(_fixture.Build<AccountModel>().Without(a => a.LegalEntityModels).Create());

            var startDate = DateTime.Now.Date;
            var dueDate = startDate.AddDays(90).Date;
            
            _sutModel = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .With(a => a.Account, new Account(_account.Id, _accountLegalEntityId))
                .With(a => a.StartDate, startDate)
                .With(a => a.PendingPaymentModels, new List<PendingPaymentModel>() 
                    {
                        _fixture.Build<PendingPaymentModel>()
                        .With(p => p.DueDate, dueDate)
                        .With(p => p.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>())
                        .With(p => p.Account, new Account(_account.Id, _accountLegalEntityId))
                        .Create()
                    })                
                .Create();

            _sut = Sut(_sutModel);
        }


        [Test]
        public void Then_an_exception_is_thrown_when_the_account_is_null()
        {
            // arrange
            var pendingPayment = _sut.PendingPayments.First();

            // act
            Action result = () => _sut.ValidateMinimumRequiredAgreementVersion(pendingPayment.Id, null, _collectionPeriod);

            // assert
            result.Should().Throw<InvalidPendingPaymentException>().WithMessage($"Unable to validate PendingPayment {pendingPayment.Id} of ApprenticeshipIncentive {_sut.Id} because the provided Account record does not match the one against the incentive.");
        }

        [Test]
        public void Then_an_exception_is_thrown_when_the_account_is_different_to_the_incentive_account()
        {
            // arrange
            var pendingPayment = _sut.PendingPayments.First();

            // act
            Action result = () => _sut.ValidateMinimumRequiredAgreementVersion(pendingPayment.Id, _fixture.Create<Accounts.Account>(), _collectionPeriod);

            // assert
            result.Should().Throw<InvalidPendingPaymentException>().WithMessage($"Unable to validate PendingPayment {pendingPayment.Id} of ApprenticeshipIncentive {_sut.Id} because the provided Account record does not match the one against the incentive.");
        }


        [TestCase(1, 2, false)]
        [TestCase(1, 1, true)]
        [TestCase(2, 1, true)]
        [TestCase(null, 1 , false)]
        [TestCase(1, null, false)]
        [TestCase(null, null, false)]        
        public void Then_a_validation_result_is_created_for_the_required_minimum_agreement_version(
            int? legalEntitySignedVersion, int? minimumRequiredVersion, bool validationResult)
        {
            // arrange               
            var pendingPayment = _sut.PendingPayments.First();

            var legalEntity = Accounts.LegalEntity.Create(_fixture.Build<LegalEntityModel>()
                .With(l => l.AccountLegalEntityId, pendingPayment.Account.AccountLegalEntityId)
                .With(l => l.SignedAgreementVersion, legalEntitySignedVersion)
                .Create());

            _account.AddLegalEntity(pendingPayment.Account.AccountLegalEntityId, legalEntity);
            
            _sutModel.MinimumAgreementVersion = new AgreementVersion(minimumRequiredVersion);

            _sut = Sut(_sutModel);

            // act
            _sut.ValidateMinimumRequiredAgreementVersion(pendingPayment.Id, _account, _collectionPeriod);

            // assert            
            pendingPayment.PendingPaymentValidationResults.Count.Should().Be(1);
            var validationresult = pendingPayment.PendingPaymentValidationResults.First();
            validationresult.Step.Should().Be(ValidationStep.HasSignedMinVersion);
            validationresult.CollectionPeriod.Should().Be(_collectionPeriod);
            validationresult.Result.Should().Be(validationResult);
        }
        
        private ApprenticeshipIncentives.ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentives.ApprenticeshipIncentive.Get(model.Id, model);
        }
    }
}
