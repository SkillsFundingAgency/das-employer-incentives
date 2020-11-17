using AutoFixture;
using Dapper.Contrib.Extensions;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Steps
{
    public partial class ValidatePaymentsSteps
    {
        private async Task CreateAccount()
        {
            if (_hasBankDetails)
            {
                _accountModel.VrfVendorId = _fixture.Create<string>(); // Invalid bank details if null
            }

            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await connection.InsertAsync(_accountModel);
        }

        private async Task CreateIncentiveWithPayments()
        {
            _applicationModel = _fixture.Build<IncentiveApplication>()
                .With(p => p.Status, IncentiveApplicationStatus.InProgress)
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
                .Create();

            _apprenticeshipsModels = _fixture.Build<IncentiveApplicationApprenticeship>()
                .With(p => p.IncentiveApplicationId, _applicationModel.Id)
                .With(p => p.PlannedStartDate, DateTime.Today.AddDays(1))
                .With(p => p.DateOfBirth, DateTime.Today.AddYears(-20))
                .With(p => p.EarningsCalculated, false)
                .CreateMany(NumberOfApprenticeships).ToList();

            _apprenticeshipIncentive = _fixture.Build<ApprenticeshipIncentive>()
                .With(p => p.IncentiveApplicationApprenticeshipId, _apprenticeshipsModels.First().Id)
                .With(p => p.AccountId, _applicationModel.AccountId)
                .With(p => p.AccountLegalEntityId, _applicationModel.AccountLegalEntityId)
                .With(p => p.ApprenticeshipId, _apprenticeshipsModels.First().ApprenticeshipId)
                .With(p => p.PlannedStartDate, DateTime.Today.AddDays(1))
                .With(p => p.DateOfBirth, DateTime.Today.AddYears(-20))
                .Create();

            _pendingPayment1 = _fixture.Build<PendingPayment>()
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.AccountId, _apprenticeshipIncentive.AccountId)
                .With(p => p.AccountLegalEntityId, _apprenticeshipIncentive.AccountLegalEntityId)
                .With(p => p.PeriodNumber, (byte?)(CollectionPeriod - 1)) // previous period
                .With(p => p.PaymentYear, CollectionPeriodYear)
                .Without(p => p.PaymentMadeDate)
                .Create();

            _pendingPayment2 = _fixture.Build<PendingPayment>()
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.AccountId, _apprenticeshipIncentive.AccountId)
                .With(p => p.AccountLegalEntityId, _apprenticeshipIncentive.AccountLegalEntityId)
                .With(p => p.PeriodNumber, CollectionPeriod) // current period
                .With(p => p.PaymentYear, CollectionPeriodYear)
                .Without(p => p.PaymentMadeDate)
                .Create();

            _pendingPayment3 = _fixture.Build<PendingPayment>()
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.AccountId, _apprenticeshipIncentive.AccountId)
                .With(p => p.AccountLegalEntityId, _apprenticeshipIncentive.AccountLegalEntityId)
                .With(p => p.PeriodNumber, (byte?)(CollectionPeriod + 1)) // next period
                .With(p => p.PaymentYear, CollectionPeriodYear)
                .Without(p => p.PaymentMadeDate)
                .Create();

            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            //await connection.InsertAsync(_accountModel);
            await connection.InsertAsync(_applicationModel);
            await connection.InsertAsync(_apprenticeshipsModels);
            await connection.InsertAsync(_apprenticeshipIncentive);
            await connection.InsertAsync(_pendingPayment1);
            await connection.InsertAsync(_pendingPayment2);
            await connection.InsertAsync(_pendingPayment3);
        }

        private async Task CreateLearnerRecord()
        {
            _learner = _fixture.Build<Learner>()
                .With(p => p.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.ULN, _apprenticeshipIncentive.ULN)
                .With(p => p.SubmissionFound, true)
                .With(p => p.InLearning, _isInLearning)
                .Create();

            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await connection.InsertAsync(_learner);
        }
    }
}
