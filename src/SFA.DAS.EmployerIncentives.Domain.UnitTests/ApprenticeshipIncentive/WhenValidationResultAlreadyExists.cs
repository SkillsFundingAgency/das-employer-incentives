using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests
{
    [TestFixture]
    public class WhenValidationResultAlreadyExists
    {
        private long _accountId;
        private long _accountLegalEntityId;
        private ApprenticeshipIncentive _sut;
        private ApprenticeshipIncentiveModel _sutModel;
        private AccountModel _accountModel;
        private Account _account;
        private CollectionPeriod _collectionPeriod;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _accountId = _fixture.Create<long>();
            _accountLegalEntityId = _fixture.Create<long>();
            var startDate = DateTime.Now.Date;
            var collectionYear = _fixture.Create<short>();
            var collectionMonth = _fixture.Create<byte>();
            
            _collectionPeriod = new CollectionPeriod(1, collectionYear);

            var legalEntityModels = new List<LegalEntityModel>
            {
                new LegalEntityModel { Id = _accountId, AccountLegalEntityId = _accountLegalEntityId }
            };
            _accountModel = new AccountModel { Id = _accountId, LegalEntityModels = legalEntityModels };
            _account = Account.Create(_accountModel);

            _sutModel = new ApprenticeshipIncentiveModel
            { 
                Account = new ApprenticeshipIncentives.ValueTypes.Account(_accountId, _accountLegalEntityId),
                StartDate = startDate,
                PendingPaymentModels = new List<PendingPaymentModel>
                {
                    new PendingPaymentModel
                    {
                        Account = new ApprenticeshipIncentives.ValueTypes.Account(_accountId, _accountLegalEntityId),
                        DueDate = startDate.AddDays(90),
                        PendingPaymentValidationResultModels = new List<PendingPaymentValidationResultModel>
                        {
                            new PendingPaymentValidationResultModel { CollectionPeriod = new CollectionPeriod(1, collectionYear), Result = false, Step = ValidationStep.HasBankDetails }, 
                            new PendingPaymentValidationResultModel { CollectionPeriod = new CollectionPeriod(2, collectionYear), Result = false, Step = ValidationStep.HasBankDetails }
                        }
                    }
                }
            };
            
            _sut = ApprenticeshipIncentive.Get(_sutModel.Id, _sutModel);
        }

        [Test]
        public void Then_the_validation_result_is_replaced()
        {
            // Arrange
            var legalEntities = new List<LegalEntityModel>
            {
                new LegalEntityModel 
                { 
                    Id = _accountId, 
                    AccountLegalEntityId = _accountLegalEntityId,
                    VrfVendorId = "P123123"
                }
            };
            _accountModel = new AccountModel {Id = _accountId, LegalEntityModels = legalEntities};
            _account = Account.Create(_accountModel);

            // Act
            _sut.ValidatePendingPaymentBankDetails(_sutModel.PendingPaymentModels.First().Id, _account, _collectionPeriod);

            // Assert
            var validationResult = _sut.PendingPayments.First().PendingPaymentValidationResults
                .SingleOrDefault(x => x.Step == ValidationStep.HasBankDetails && x.CollectionPeriod.PeriodNumber == _collectionPeriod.PeriodNumber && x.CollectionPeriod.AcademicYear == _collectionPeriod.AcademicYear);
            validationResult.Should().NotBeNull();
            validationResult.Result.Should().BeTrue();
        }
    }
}
