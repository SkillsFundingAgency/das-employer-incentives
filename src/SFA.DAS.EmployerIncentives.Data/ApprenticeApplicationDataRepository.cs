using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;

namespace SFA.DAS.EmployerIncentives.Data
{
    public class ApprenticeApplicationDataRepository : IApprenticeApplicationDataRepository
    {
        private readonly EmployerIncentivesDbContext _dbContext;
        private readonly IDateTimeService _dateTimeService;
        private readonly ICollectionCalendarService _collectionCalendarService;

        public ApprenticeApplicationDataRepository(
            Lazy<EmployerIncentivesDbContext> dbContext,
            IDateTimeService dateTimeService,
            ICollectionCalendarService collectionCalendarService)
        {
            _dbContext = dbContext.Value;
            _dateTimeService = dateTimeService;
            _collectionCalendarService = collectionCalendarService;
        }

        public async Task<List<ApprenticeApplicationDto>> GetList(long accountId, long accountLegalEntityId)
        {
            var calendar = await _collectionCalendarService.Get();
            var nextActivePeriod = calendar.GetNextPeriod(calendar.GetActivePeriod());

            var accountApplications = _dbContext.ApprenticeApplications.Where(x =>
                x.AccountId == accountId && x.AccountLegalEntityId == accountLegalEntityId);
            
            var result = new List<ApprenticeApplicationDto>();

            foreach (var data in accountApplications)
            {
                var apprenticeApplicationDto = new ApprenticeApplicationDto
                {
                    AccountId = data.AccountId,
                    AccountLegalEntityId = data.AccountLegalEntityId,
                    ApplicationDate = data.SubmittedDate,
                    FirstName = data.FirstName,
                    LastName = data.LastName,
                    ULN = data.ULN,
                    LegalEntityName = data.LegalEntityName,
                    SubmittedByEmail = data.SubmittedByEmail,
                    TotalIncentiveAmount = CalculateTotalIncentiveAmount(data.FirstPendingPaymentAmount, data.SecondPendingPaymentAmount),
                    CourseName = data.CourseName,
                    FirstPaymentStatus = data.FirstPendingPaymentAmount == default ? null : new PaymentStatusDto
                    {
                        PaymentDate = PaymentDate(data.FirstPendingPaymentDueDate, data.FirstPaymentDate, data.FirstPaymentCalculatedDate, nextActivePeriod),
                        LearnerMatchFound = data.LearningFound.HasValue && data.LearningFound.Value,
                        PaymentAmount = PaymentAmount(data.FirstPendingPaymentAmount, data.FirstPaymentAmount),
                        HasDataLock = data.HasDataLock.HasValue && data.HasDataLock.Value,
                        InLearning = data.InLearning.HasValue && data.InLearning.Value,
                        PausePayments = data.PausePayments,
                        PaymentSent = data.FirstPaymentDate.HasValue,
                        PaymentSentIsEstimated = data.IsPaymentEstimated(EarningType.FirstPayment, _dateTimeService),
                        RequiresNewEmployerAgreement = !data.SignedAgreementVersion.HasValue || data.SignedAgreementVersion < data.MinimumAgreementVersion,
                        EmploymentCheckPassed = EmploymentCheckResult(data.FirstEmploymentCheckResult, data.FirstEmploymentCheckValidation, data.SecondEmploymentCheckResult, data.SecondEmploymentCheckValidation)
                    },
                    FirstClawbackStatus = data.FirstClawbackAmount == default ? null : new ClawbackStatusDto
                    {
                        ClawbackAmount = data.FirstClawbackAmount.Value,
                        ClawbackDate = data.FirstClawbackCreated,
                        OriginalPaymentDate = data.FirstPaymentDate
                    },
                    SecondPaymentStatus = data.SecondPendingPaymentAmount == default ? null : new PaymentStatusDto
                    {
                        PaymentDate = PaymentDate(data.SecondPendingPaymentDueDate, data.SecondPaymentDate, data.SecondPaymentCalculatedDate, nextActivePeriod),
                        LearnerMatchFound = data.LearningFound.HasValue && data.LearningFound.Value,
                        PaymentAmount = PaymentAmount(data.SecondPendingPaymentAmount, data.SecondPaymentAmount),
                        HasDataLock = data.HasDataLock.HasValue && data.HasDataLock.Value,
                        InLearning = data.InLearning.HasValue && data.InLearning.Value,
                        PausePayments = data.PausePayments,
                        PaymentSent = data.SecondPaymentDate.HasValue,
                        PaymentSentIsEstimated = data.IsPaymentEstimated(EarningType.SecondPayment, _dateTimeService),
                        RequiresNewEmployerAgreement = !data.SignedAgreementVersion.HasValue || data.SignedAgreementVersion < data.MinimumAgreementVersion,
                        EmploymentCheckPassed = EmploymentCheckResult(data.FirstEmploymentCheckResult, data.FirstEmploymentCheckValidation, data.SecondEmploymentCheckResult, data.SecondEmploymentCheckValidation)
                    },
                    SecondClawbackStatus = data.SecondClawbackAmount == default ? null : new ClawbackStatusDto
                    {
                        ClawbackAmount = data.SecondClawbackAmount.Value,
                        ClawbackDate = data.SecondClawbackCreated,
                        OriginalPaymentDate = data.SecondPaymentDate
                    },
                };

                if (data.Status == IncentiveStatus.Stopped)
                {
                    SetStoppedStatus(apprenticeApplicationDto);
                } 
                else if (data.Status == IncentiveStatus.Withdrawn)
                {
                    SetWithdrawnStatus(apprenticeApplicationDto, data.WithdrawnBy.Value);
                }
                
                result.Add(apprenticeApplicationDto);
            }

            return result;
        }

        private decimal CalculateTotalIncentiveAmount(decimal? firstPendingPaymentAmount, decimal? secondPendingPaymentAmount)
        {
            var amount = 0m;
            if (firstPendingPaymentAmount.HasValue)
            {
                amount += firstPendingPaymentAmount.Value;
            }
            if (secondPendingPaymentAmount.HasValue)
            {
                amount += secondPendingPaymentAmount.Value;
            }

            return amount;
        }

        private static bool? EmploymentCheckResult(bool? firstEmploymentCheck, bool? firstEmploymentCheckValidation, bool? secondEmploymentCheck, bool? secondEmploymentCheckValidation)
        {
            if (!firstEmploymentCheck.HasValue
                || !secondEmploymentCheck.HasValue
                || !firstEmploymentCheckValidation.HasValue 
                || !secondEmploymentCheckValidation.HasValue
                )
            {
                return null;
            }

            return firstEmploymentCheckValidation.Value && secondEmploymentCheckValidation.Value;
        }

        private static void SetStoppedStatus(ApprenticeApplicationDto model)
        {
            var paymentStatus = new PaymentStatusDto { PaymentIsStopped = true };
            SetIncentiveStatus(paymentStatus, model);
        }

        private static void SetWithdrawnStatus(ApprenticeApplicationDto model, WithdrawnBy withdrawnBy)
        {
            var paymentStatus = new PaymentStatusDto { WithdrawnByCompliance = withdrawnBy == WithdrawnBy.Compliance, WithdrawnByEmployer = withdrawnBy == WithdrawnBy.Employer};
            SetIncentiveStatus(paymentStatus, model);
        }

        private static void SetIncentiveStatus(PaymentStatusDto paymentStatus, ApprenticeApplicationDto model)
        {
            if (model.FirstPaymentStatus == null)
            {
                if (model.SecondPaymentStatus == null)
                {
                    model.FirstPaymentStatus = paymentStatus;
                    model.SecondPaymentStatus = paymentStatus;
                }
            }
            else
            {
                if (model.SecondPaymentStatus == null)
                {
                    model.SecondPaymentStatus = paymentStatus;
                }
            }
        }
        
        private static DateTime? PaymentDate(
            DateTime? pendingPaymentDate,
            DateTime? paymentSentDate,
            DateTime? paymentCalculatedDate,
            Domain.ValueObjects.CollectionCalendarPeriod nextActivePeriod)
        {

            if (paymentSentDate.HasValue)
            {
                return paymentSentDate.Value;
            }
            else if (paymentCalculatedDate.HasValue)
            {
                return paymentCalculatedDate.Value;
            }
            
            if (!pendingPaymentDate.HasValue)
            {
                return default;
            }

            var activePeriodDate = new DateTime(nextActivePeriod.OpenDate.Year, nextActivePeriod.OpenDate.Month, nextActivePeriod.OpenDate.Day);
            var paymentDueDate = new DateTime(pendingPaymentDate.Value.Year, pendingPaymentDate.Value.Month, pendingPaymentDate.Value.Day);

            if (paymentDueDate < activePeriodDate)
            {
                return new DateTime(nextActivePeriod.CalendarYear, nextActivePeriod.CalendarMonth, 27);
            }
            return pendingPaymentDate.Value.AddMonths(1);
        }

        private static decimal? PaymentAmount(decimal? pendingPaymentAmount, decimal? paymentAmount)
        {
            if (paymentAmount.HasValue)
            {
                return paymentAmount;
            }
            return pendingPaymentAmount;
        }

        public async Task<Guid?> GetFirstSubmittedApplicationId(long accountLegalEntityId)
        {
            var firstSubmittedApplicationId = await _dbContext.Applications
                .Where(x => x.AccountLegalEntityId == accountLegalEntityId && x.Status == IncentiveApplicationStatus.Submitted)
                .OrderBy(x => x.DateSubmitted)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            return firstSubmittedApplicationId;
        }
    }
}
