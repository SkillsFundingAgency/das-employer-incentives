using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using ValidationOverride = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.ValidationOverride;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries;
using ApprenticeApplication = SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeApplication;

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

        public async Task<List<ApprenticeApplication>> GetList(long accountId, long accountLegalEntityId)
        {
            var calendar = await _collectionCalendarService.Get();
            var nextActivePeriod = calendar.GetNextPeriod(calendar.GetActivePeriod());

            var accountApplications = _dbContext.ApprenticeApplications.Where(x =>
                x.AccountId == accountId && x.AccountLegalEntityId == accountLegalEntityId);

            var apprenticeshipIncentives = _dbContext.ApprenticeshipIncentives.Where(x => x.AccountId == accountId && x.AccountLegalEntityId == accountLegalEntityId).Include(i => i.ValidationOverrides);

            var result = new List<ApprenticeApplication>();

            foreach (var data in accountApplications)
            {
                var validationOverrides = apprenticeshipIncentives.Single(i => i.Id == data.Id).ValidationOverrides;

                var apprenticeApplicationDto = new ApprenticeApplication
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
                    FirstPaymentStatus = data.FirstPendingPaymentAmount == default ? null : new PaymentStatus
                    {
                        PaymentDate = PaymentDate(data.FirstPendingPaymentDueDate, data.FirstPaymentDate, data.FirstPaymentCalculatedDate, nextActivePeriod),
                        LearnerMatchFound = data.LearningFound.HasValue && data.LearningFound.Value,
                        PaymentAmount = PaymentAmount(data.FirstPendingPaymentAmount, data.FirstPaymentAmount),
                        HasDataLock = HasDataLockOverride(validationOverrides, data.HasDataLock.HasValue && data.HasDataLock.Value),
                        InLearning = IsInLearningOverride(validationOverrides, data.InLearning.HasValue && data.InLearning.Value),
                        PausePayments = data.PausePayments,
                        PaymentSent = data.FirstPaymentDate.HasValue,
                        PaymentSentIsEstimated = data.IsPaymentEstimated(EarningType.FirstPayment, _dateTimeService),
                        RequiresNewEmployerAgreement = !data.SignedAgreementVersion.HasValue || data.SignedAgreementVersion < data.MinimumAgreementVersion,
                        EmploymentCheckPassed = EmploymentCheckResult(
                                        EmployedAtStartOfApprenticeshipOverride(validationOverrides, data.FirstEmploymentCheckResult, data.FirstEmploymentCheckValidation, EmployedAtStartOfApprenticeship),
                                        EmployedBeforeSchemeStartedOverride(validationOverrides, data.SecondEmploymentCheckResult, data.SecondEmploymentCheckValidation, EmployedBeforeSchemeStarted)),
                        EmploymentCheckErrorCodes = EmploymentCheckErrorCodes(data.FirstEmploymentCheckErrorType, data.SecondEmploymentCheckErrorType)
                    },
                    FirstClawbackStatus = data.FirstClawbackAmount == default ? null : new ClawbackStatus
                    {
                        ClawbackAmount = data.FirstClawbackAmount.Value,
                        ClawbackDate = data.FirstClawbackCreated,
                        OriginalPaymentDate = data.FirstPaymentDate
                    },
                    SecondPaymentStatus = data.SecondPendingPaymentAmount == default ? null : new PaymentStatus
                    {
                        PaymentDate = PaymentDate(data.SecondPendingPaymentDueDate, data.SecondPaymentDate, data.SecondPaymentCalculatedDate, nextActivePeriod),
                        LearnerMatchFound = data.LearningFound.HasValue && data.LearningFound.Value,
                        PaymentAmount = PaymentAmount(data.SecondPendingPaymentAmount, data.SecondPaymentAmount),
                        HasDataLock = HasDataLockOverride(validationOverrides, data.HasDataLock.HasValue && data.HasDataLock.Value),
                        InLearning = IsInLearningOverride(validationOverrides, data.InLearning.HasValue && data.InLearning.Value),
                        PausePayments = data.PausePayments,
                        PaymentSent = data.SecondPaymentDate.HasValue,
                        PaymentSentIsEstimated = data.IsPaymentEstimated(EarningType.SecondPayment, _dateTimeService),
                        RequiresNewEmployerAgreement = !data.SignedAgreementVersion.HasValue || data.SignedAgreementVersion < data.MinimumAgreementVersion,
                        EmploymentCheckPassed = EmploymentCheckResult(
                                        EmployedAtStartOfApprenticeshipOverride(validationOverrides, data.FirstEmploymentCheckResult, data.FirstEmploymentCheckValidation, EmployedAtStartOfApprenticeship),
                                        EmployedBeforeSchemeStartedOverride(validationOverrides, data.SecondEmploymentCheckResult, data.SecondEmploymentCheckValidation, EmployedBeforeSchemeStarted)),
                        EmploymentCheckErrorCodes = EmploymentCheckErrorCodes(data.FirstEmploymentCheckErrorType, data.SecondEmploymentCheckErrorType)
                    },
                    SecondClawbackStatus = data.SecondClawbackAmount == default ? null : new ClawbackStatus
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

        private static bool HasDataLockOverride(
            IEnumerable<ValidationOverride> validationOverrides,
            bool hasDataLock)
        {
            if (validationOverrides.Any(x => x.Step == ValidationStep.HasNoDataLocks
                                                  && x.ExpiryDate.Date > DateTime.UtcNow.Date))
            {
                return false;
            }

            return hasDataLock;
        }

        private static bool IsInLearningOverride(
            IEnumerable<ValidationOverride> validationOverrides,
            bool isInLearning)
        {
            if (validationOverrides.Any(x => x.Step == ValidationStep.IsInLearning
                                                  && x.ExpiryDate.Date > DateTime.UtcNow.Date))
            {
                return true;
            }

            return isInLearning;
        }

        private static bool? EmploymentCheckResult(bool? firstEmploymentCheck, bool? secondEmploymentCheck)
        {
            if (firstEmploymentCheck == null || secondEmploymentCheck == null)
            {
                return null;
            }

            return firstEmploymentCheck.Value && secondEmploymentCheck.Value;
        }

        private static bool? EmployedAtStartOfApprenticeship(bool? firstEmploymentCheck,
                                                    bool? firstEmploymentCheckValidation)
        {
            if (!firstEmploymentCheck.HasValue
                || !firstEmploymentCheckValidation.HasValue
                )
            {
                return null;
            }

            return firstEmploymentCheckValidation.Value;
        }        

        private static bool? EmployedAtStartOfApprenticeshipOverride(
            IEnumerable<ValidationOverride> validationOverrides,
            bool? firstEmploymentCheck,
            bool? firstEmploymentCheckValidation,
            Func<bool?, bool?, bool?> func)
        {
            if (validationOverrides.Any(x => x.Step == ValidationStep.EmployedAtStartOfApprenticeship
                                                  && x.ExpiryDate.Date > DateTime.UtcNow.Date))
            {
                return true;
            }

            return func.Invoke(firstEmploymentCheck, firstEmploymentCheckValidation);
        }

        private static bool? EmployedBeforeSchemeStarted(
           bool? secondEmploymentCheck,
           bool? secondEmploymentCheckValidation)
        {
            if (!secondEmploymentCheck.HasValue
                || !secondEmploymentCheckValidation.HasValue
                )
            {
                return null;
            }

            return secondEmploymentCheckValidation.Value;
        }

        private static List<string> EmploymentCheckErrorCodes(EmploymentCheckResultError? firstEmploymentCheckErrorCode, EmploymentCheckResultError? secondEmploymentCheckErrorCode)
        {
            var errorCodes = new List<string>();
            if (firstEmploymentCheckErrorCode != null && !errorCodes.Contains(firstEmploymentCheckErrorCode.ToString()))
            {
                errorCodes.Add(firstEmploymentCheckErrorCode.ToString());
            }

            if (secondEmploymentCheckErrorCode != null && !errorCodes.Contains(secondEmploymentCheckErrorCode.ToString()))
            {
                errorCodes.Add(secondEmploymentCheckErrorCode.ToString());
            }

            return errorCodes;
        }

        private static bool? EmployedBeforeSchemeStartedOverride(
            IEnumerable<ValidationOverride> validationOverrides,
            bool? secondEmploymentCheck,
            bool? secondEmploymentCheckValidation,
            Func<bool?, bool?, bool?> func)
        {
            if (validationOverrides.Any(x => x.Step == ValidationStep.EmployedBeforeSchemeStarted
                                                  && x.ExpiryDate.Date > DateTime.UtcNow.Date))
            {
                return true;
            }

            return func.Invoke(secondEmploymentCheck, secondEmploymentCheckValidation);
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

        private static void SetStoppedStatus(ApprenticeApplication model)
        {
            var paymentStatus = new PaymentStatus { PaymentIsStopped = true };
            SetIncentiveStatus(paymentStatus, model);
        }

        private static void SetWithdrawnStatus(ApprenticeApplication model, WithdrawnBy withdrawnBy)
        {
            var paymentStatus = new PaymentStatus { WithdrawnByCompliance = withdrawnBy == WithdrawnBy.Compliance, WithdrawnByEmployer = withdrawnBy == WithdrawnBy.Employer};
            SetIncentiveStatus(paymentStatus, model);
        }

        private static void SetIncentiveStatus(PaymentStatus paymentStatus, ApprenticeApplication model)
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
                model.SecondPaymentStatus = paymentStatus;
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
