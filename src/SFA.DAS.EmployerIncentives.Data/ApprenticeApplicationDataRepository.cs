using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using Learner = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.Learner;
using Payment = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.Payment;
using PendingPayment = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.PendingPayment;

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

            var accountApplications = from incentive in _dbContext.ApprenticeshipIncentives
                                      from account in _dbContext.Accounts.Where(x => x.AccountLegalEntityId == incentive.AccountLegalEntityId)
                                      from firstPayment in _dbContext.PendingPayments.Where(x => x.ApprenticeshipIncentiveId == incentive.Id && x.EarningType == EarningType.FirstPayment && !x.ClawedBack).DefaultIfEmpty()
                                      from firstPaymentClawedback in _dbContext.PendingPayments.Where(x => x.ApprenticeshipIncentiveId == incentive.Id && x.EarningType == EarningType.FirstPayment && x.ClawedBack).DefaultIfEmpty()
                                      from firstClawback in _dbContext.ClawbackPayments.Where(x => x.PendingPaymentId == firstPaymentClawedback.Id).DefaultIfEmpty()
                                      from firstClawbackPayment in _dbContext.Payments.Where(x => x.Id == firstClawback.PaymentId).DefaultIfEmpty()
                                      from secondPayment in _dbContext.PendingPayments.Where(x => x.ApprenticeshipIncentiveId == incentive.Id && x.EarningType == EarningType.SecondPayment && !x.ClawedBack).DefaultIfEmpty()
                                      from secondPaymentClawedback in _dbContext.PendingPayments.Where(x => x.ApprenticeshipIncentiveId == incentive.Id && x.EarningType == EarningType.SecondPayment && x.ClawedBack).DefaultIfEmpty()
                                      from secondClawback in _dbContext.ClawbackPayments.Where(x => x.PendingPaymentId == secondPaymentClawedback.Id).DefaultIfEmpty()
                                      from secondClawbackPayment in _dbContext.Payments.Where(x => x.Id == secondClawback.PaymentId).DefaultIfEmpty()
                                      from firstPaymentSent in _dbContext.Payments.Where(x => x.ApprenticeshipIncentiveId == incentive.Id && x.PendingPaymentId == (firstPayment == null ? Guid.Empty : firstPayment.Id)).DefaultIfEmpty()
                                      from secondPaymentSent in _dbContext.Payments.Where(x => x.ApprenticeshipIncentiveId == incentive.Id && x.PendingPaymentId == (secondPayment == null ? Guid.Empty : secondPayment.Id)).DefaultIfEmpty()
                                      from learner in _dbContext.Learners.Where(x => x.ApprenticeshipIncentiveId == incentive.Id).DefaultIfEmpty()
                                      from firstEmploymentCheck in _dbContext.PendingPaymentValidationResults.Where(x => x.PendingPaymentId == firstPayment.Id || x.PendingPaymentId == secondPayment.Id && x.Step == ValidationStep.EmployedAtStartOfApprenticeship).OrderByDescending(x => x.CreatedDateUtc).DefaultIfEmpty()
                                      from secondEmploymentCheck in _dbContext.PendingPaymentValidationResults.Where(x => x.PendingPaymentId == firstPayment.Id || x.PendingPaymentId == secondPayment.Id && x.Step == ValidationStep.EmployedBeforeSchemeStarted).OrderByDescending(x => x.CreatedDateUtc).DefaultIfEmpty()
                                      where incentive.AccountId == accountId && incentive.AccountLegalEntityId == accountLegalEntityId
                                      select new { incentive, account, firstPayment, secondPayment, learner, firstPaymentSent, 
                                                   firstClawback, firstClawbackPayment, secondClawback, secondClawbackPayment, secondPaymentSent,
                                                   firstEmploymentCheck, secondEmploymentCheck};
            
            var result = new List<ApprenticeApplicationDto>();

            foreach (var data in accountApplications)
            {
                var apprenticeApplicationDto = new ApprenticeApplicationDto
                {
                    AccountId = data.incentive.AccountId,
                    AccountLegalEntityId = data.incentive.AccountLegalEntityId,
                    ApplicationDate = data.incentive.SubmittedDate ?? DateTime.Now,
                    FirstName = data.incentive.FirstName,
                    LastName = data.incentive.LastName,
                    ULN = data.incentive.ULN,
                    LegalEntityName = data.account.LegalEntityName,
                    SubmittedByEmail = data.incentive.SubmittedByEmail,
                    TotalIncentiveAmount = data.incentive.PendingPayments.Sum(x => x.Amount),
                    CourseName = data.incentive.CourseName,
                    FirstPaymentStatus = data.firstPayment == default ? null : new PaymentStatusDto
                    {
                        PaymentDate = PaymentDate(data.firstPayment, data.firstPaymentSent, nextActivePeriod),
                        LearnerMatchFound = LearnerMatchFound(data.learner),
                        PaymentAmount = PaymentAmount(data.firstPayment, data.firstPaymentSent),
                        HasDataLock = HasDataLock(data.learner),
                        InLearning = InLearning(data.learner),
                        PausePayments = data.incentive.PausePayments,
                        PaymentSent = data.firstPaymentSent != null,
                        PaymentSentIsEstimated = IsPaymentEstimated(data.firstPaymentSent, _dateTimeService),
                        RequiresNewEmployerAgreement = !data.account.SignedAgreementVersion.HasValue || data.account.SignedAgreementVersion < data.incentive.MinimumAgreementVersion,
                        EmploymentCheckPassed = (data.firstEmploymentCheck != null && data.secondEmploymentCheck != null) && data.firstEmploymentCheck.Result && data.secondEmploymentCheck.Result
                    },
                    FirstClawbackStatus = data.firstClawback == default ? null : new ClawbackStatusDto
                    {
                        ClawbackAmount = data.firstClawback.Amount,
                        ClawbackDate = data.firstClawback.DateClawbackCreated,
                        OriginalPaymentDate = data.firstClawbackPayment?.PaidDate
                    },
                    SecondPaymentStatus = data.secondPayment == default ? null : new PaymentStatusDto
                    {
                        PaymentDate = PaymentDate(data.secondPayment, data.secondPaymentSent, nextActivePeriod),
                        LearnerMatchFound = LearnerMatchFound(data.learner),
                        PaymentAmount = data.secondPayment.Amount,
                        HasDataLock = HasDataLock(data.learner),
                        InLearning = InLearning(data.learner),
                        PausePayments = data.incentive.PausePayments,
                        PaymentSent = data.secondPaymentSent != null,
                        PaymentSentIsEstimated = IsPaymentEstimated(data.secondPaymentSent, _dateTimeService),
                        RequiresNewEmployerAgreement = !data.account.SignedAgreementVersion.HasValue || data.account.SignedAgreementVersion < data.incentive.MinimumAgreementVersion,
                        EmploymentCheckPassed = (data.firstEmploymentCheck != null && data.secondEmploymentCheck != null) && data.firstEmploymentCheck.Result && data.secondEmploymentCheck.Result
                    },
                    SecondClawbackStatus = data.secondClawback == default ? null : new ClawbackStatusDto
                    {
                        ClawbackAmount = data.secondClawback.Amount,
                        ClawbackDate = data.secondClawback.DateClawbackCreated,
                        OriginalPaymentDate = data.secondClawbackPayment?.PaidDate
                    },
                };

                if (data.incentive.Status == IncentiveStatus.Stopped)
                {
                    SetStoppedStatus(apprenticeApplicationDto);
                } 
                else if (data.incentive.Status == IncentiveStatus.Withdrawn)
                {
                    SetWithdrawnStatus(apprenticeApplicationDto, data.incentive.WithdrawnBy.Value);
                }
                
                result.Add(apprenticeApplicationDto);
            }

            return result;
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

        private static bool LearnerMatchFound(Learner learner)
        {
            if(learner == null)
            {
                return false;
            }

            return learner.LearningFound.HasValue && learner.LearningFound.Value;
        }

        private static bool HasDataLock(Learner learner)
        {
            if (learner == null)
            {
                return false;
            }

            return learner.HasDataLock.HasValue && learner.HasDataLock.Value;
        }

        private static bool InLearning(Learner learner)
        {
            if (learner == null)
            {
                return false;
            }

            return learner.InLearning.HasValue && learner.InLearning.Value;
        }
        private static DateTime? PaymentDate(
            PendingPayment pendingPayment, 
            Payment payment,
            Domain.ValueObjects.CollectionCalendarPeriod nextActivePeriod)
        {
            if (payment != null)
            {
                if (payment.PaidDate != null)
                {
                    return payment.PaidDate.Value;
                }
                return payment.CalculatedDate;
            }
            
            var activePeriodDate = new DateTime(nextActivePeriod.OpenDate.Year, nextActivePeriod.OpenDate.Month, nextActivePeriod.OpenDate.Day);
            var paymentDueDate = new DateTime(pendingPayment.DueDate.Year, pendingPayment.DueDate.Month, pendingPayment.DueDate.Day);

            if (paymentDueDate < activePeriodDate)
            {
                return new DateTime(nextActivePeriod.CalendarYear, nextActivePeriod.CalendarMonth, 27);
            }
            return pendingPayment.DueDate.AddMonths(1);
        }

        private static decimal? PaymentAmount(PendingPayment pendingPayment, Payment payment)
        {
            if (payment != null)
            {
                return payment.Amount;
            }
            return pendingPayment.Amount;
        }
        
        private static bool IsPaymentEstimated(Payment payment, IDateTimeService dateTimeService)
        {
            if(payment == null || !payment.PaidDate.HasValue)
            {
                return true;
            }

            if (dateTimeService.Now().Day < 27 &&
                payment.PaidDate.Value.Year == dateTimeService.Now().Year &&
                payment.PaidDate.Value.Month == dateTimeService.Now().Month)
            {
                return true;
            }
            return false;
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
