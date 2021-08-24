using AutoFixture;
using Dapper.Contrib.Extensions;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Steps
{
    public partial class ValidatePaymentsSteps
    {
        public class ValidatePaymentData
        {
            public const int NumberOfApprenticeships = 3;
            public Account AccountModel { get; }
            public IncentiveApplication IncentiveApplicationModel { get; }
            public List<IncentiveApplicationApprenticeship> IncentiveApplicationApprenticeshipModels { get; }
            public ApprenticeshipIncentive ApprenticeshipIncentiveModel { get; }
            public PendingPayment PendingPaymentModel1 { get; }
            public PendingPayment PendingPaymentModel2 { get; }
            public PendingPayment PendingPaymentModel3 { get; }
            public Learner LearnerModel { get; }
            public ApprenticeshipDaysInLearning DaysInLearning { get; }
            public Payment Payment { get; private set; }
            private ClawbackPayment ClawbackPayment { get; set; }
            private readonly TestContext _testContext;
            private readonly Fixture _fixture;

            public ValidatePaymentData(TestContext testContext)
            {
                _testContext = testContext;
                _fixture = new Fixture();

                AccountModel = _fixture.Create<Account>();
                var startDate = DateTime.Today.AddDays(1);
                var dueDate = startDate.AddDays(90);
                AccountModel.SignedAgreementVersion = 5;

                IncentiveApplicationModel = _fixture.Build<IncentiveApplication>()
                .With(p => p.Status, IncentiveApplicationStatus.InProgress)
                .With(p => p.AccountId, AccountModel.Id)
                .With(p => p.AccountLegalEntityId, AccountModel.AccountLegalEntityId)
                .Create();

                IncentiveApplicationApprenticeshipModels = _fixture.Build<IncentiveApplicationApprenticeship>()
                .With(p => p.IncentiveApplicationId, IncentiveApplicationModel.Id)
                .With(p => p.PlannedStartDate, startDate)
                .With(p => p.DateOfBirth, DateTime.Today.AddYears(-20))
                .With(p => p.EarningsCalculated, false)
                .CreateMany(NumberOfApprenticeships).ToList();

                ApprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentive>()
                    .With(p => p.IncentiveApplicationApprenticeshipId, IncentiveApplicationApprenticeshipModels.First().Id)
                    .With(p => p.AccountId, IncentiveApplicationModel.AccountId)
                    .With(p => p.AccountLegalEntityId, IncentiveApplicationModel.AccountLegalEntityId)
                    .With(p => p.ApprenticeshipId, IncentiveApplicationApprenticeshipModels.First().ApprenticeshipId)
                    .With(p => p.StartDate, startDate)
                    .With(p => p.DateOfBirth, startDate.AddYears(-20))
                    .With(p => p.PausePayments, false)
                    .With(p => p.MinimumAgreementVersion, AccountModel.SignedAgreementVersion)
                    .Create();

                PendingPaymentModel1 = _fixture.Build<PendingPayment>()
                    .With(p => p.ApprenticeshipIncentiveId, ApprenticeshipIncentiveModel.Id)
                    .With(p => p.AccountId, ApprenticeshipIncentiveModel.AccountId)
                    .With(p => p.AccountLegalEntityId, ApprenticeshipIncentiveModel.AccountLegalEntityId)
                    .With(p => p.EarningType, EarningType.FirstPayment)
                    .With(p => p.DueDate, dueDate)
                    .With(p => p.PeriodNumber, (byte?)(CollectionPeriod - 1)) // previous period
                    .With(p => p.PaymentYear, CollectionPeriodYear)
                    .Without(p => p.PaymentMadeDate)
                    .Create();

                PendingPaymentModel2 = _fixture.Build<PendingPayment>()
                    .With(p => p.ApprenticeshipIncentiveId, ApprenticeshipIncentiveModel.Id)
                    .With(p => p.AccountId, ApprenticeshipIncentiveModel.AccountId)
                    .With(p => p.AccountLegalEntityId, ApprenticeshipIncentiveModel.AccountLegalEntityId)
                    .With(p => p.EarningType, EarningType.FirstPayment)
                    .With(p => p.DueDate, dueDate)
                    .With(p => p.PeriodNumber, CollectionPeriod) // current period
                    .With(p => p.PaymentYear, CollectionPeriodYear)
                    .Without(p => p.PaymentMadeDate)
                    .Create();

                PendingPaymentModel3 = _fixture.Build<PendingPayment>()
                    .With(p => p.ApprenticeshipIncentiveId, ApprenticeshipIncentiveModel.Id)
                    .With(p => p.AccountId, ApprenticeshipIncentiveModel.AccountId)
                    .With(p => p.AccountLegalEntityId, ApprenticeshipIncentiveModel.AccountLegalEntityId)
                    .With(p => p.EarningType, EarningType.FirstPayment)
                    .With(p => p.DueDate, startDate.AddDays(89))
                    .With(p => p.PeriodNumber, (byte?)(CollectionPeriod + 1)) // next period
                    .With(p => p.PaymentYear, CollectionPeriodYear)
                    .Without(p => p.PaymentMadeDate)
                    .Create();

                LearnerModel = _fixture.Build<Learner>()
                    .With(l => l.ApprenticeshipId, ApprenticeshipIncentiveModel.ApprenticeshipId)
                    .With(l => l.ApprenticeshipIncentiveId, ApprenticeshipIncentiveModel.Id)
                    .With(l => l.CreatedDate, DateTime.Today.AddDays(-1))
                    .With(l => l.ULN, ApprenticeshipIncentiveModel.ULN)
                    .With(l => l.SuccessfulLearnerMatchExecution, true)
                    .With(l => l.SubmissionFound, true)
                    .With(l => l.LearningFound, true)
                    .With(l => l.InLearning, true)
                    .With(l => l.HasDataLock, false)
                    .With(l => l.StartDate, DateTime.Today.AddDays(-100))
                    .Create();

                DaysInLearning = _fixture.Build<ApprenticeshipDaysInLearning>()
                    .With(d => d.LearnerId, LearnerModel.Id)
                    .With(d => d.NumberOfDaysInLearning, 90)
                    .With(d => d.CollectionPeriodNumber, CollectionPeriod) // current period
                    .With(d => d.CollectionPeriodYear, CollectionPeriodYear)
                    .Create();

            }

            public async Task Create()
            {
                await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
                await connection.InsertAsync(AccountModel);
                await connection.InsertAsync(IncentiveApplicationModel);
                await connection.InsertAsync(IncentiveApplicationApprenticeshipModels);
                await connection.InsertAsync(ApprenticeshipIncentiveModel);
                await connection.InsertAsync(PendingPaymentModel1);
                await connection.InsertAsync(PendingPaymentModel2);
                await connection.InsertAsync(PendingPaymentModel3);
                await connection.InsertAsync(LearnerModel);
                await connection.InsertAsync(DaysInLearning);

                if (Payment != default) await connection.InsertAsync(Payment);
                if (ClawbackPayment != default) await connection.InsertAsync(ClawbackPayment);
            }

            internal void AddClawbackPayment(bool sent)
            {
                Payment = _fixture.Build<Payment>()
                    .With(d => d.PendingPaymentId, PendingPaymentModel2.Id)
                    .With(d => d.AccountId, AccountModel.Id)
                    .With(d => d.AccountLegalEntityId, AccountModel.AccountLegalEntityId)
                    .With(d => d.ApprenticeshipIncentiveId, ApprenticeshipIncentiveModel.Id)
                    .With(d => d.PaymentPeriod, PendingPaymentModel2.PeriodNumber)
                    .With(d => d.PaymentYear, PendingPaymentModel2.PaymentYear)
                    .Without(d => d.PaidDate)
                    .Create();

                ClawbackPayment = _fixture.Build<ClawbackPayment>()
                    .With(d => d.PendingPaymentId, PendingPaymentModel2.Id)
                    .With(d => d.PaymentId, Payment.Id)
                    .With(d => d.DateClawbackSent, sent ? DateTime.Now.AddDays(-1) : (DateTime?)null)
                    .With(d => d.AccountId, AccountModel.Id)
                    .With(d => d.AccountLegalEntityId, AccountModel.AccountLegalEntityId)
                    .With(d => d.ApprenticeshipIncentiveId, ApprenticeshipIncentiveModel.Id)
                    .With(d => d.CollectionPeriod, PendingPaymentModel2.PeriodNumber)
                    .With(d => d.CollectionPeriodYear, PendingPaymentModel2.PaymentYear)
                    .Create();
            }
        }
    }
}
