using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Map
{
    public static class ArchiveMappingExtensions
    {
        public static ArchivedPayment Archive(this Payment payment)
        {
            return new ArchivedPayment
            {
                PaymentId = payment.Id,
                PendingPaymentId = payment.PendingPaymentId,
                PaymentYear = payment.PaymentYear,
                PaymentPeriod = payment.PaymentPeriod,
                CalculatedDate = payment.CalculatedDate,
                Amount = payment.Amount,
                ApprenticeshipIncentiveId = payment.ApprenticeshipIncentiveId,
                AccountLegalEntityId = payment.AccountLegalEntityId,
                AccountId = payment.AccountId,
                PaidDate = payment.PaidDate,
                SubnominalCode = payment.SubnominalCode
            };
        }

        public static ArchivedPendingPayment Archive(this PendingPayment pendingPayment)
        {
            return new ArchivedPendingPayment
            {
                PendingPaymentId = pendingPayment.Id,
                PaymentYear = pendingPayment.PaymentYear,
                PeriodNumber = pendingPayment.PeriodNumber,
                CalculatedDate = pendingPayment.CalculatedDate,
                Amount = pendingPayment.Amount,
                ApprenticeshipIncentiveId = pendingPayment.ApprenticeshipIncentiveId,
                AccountLegalEntityId = pendingPayment.AccountLegalEntityId,
                AccountId = pendingPayment.AccountId,
                ClawedBack = pendingPayment.ClawedBack,
                DueDate = pendingPayment.DueDate,
                EarningType = pendingPayment.EarningType,
                PaymentMadeDate = pendingPayment.PaymentMadeDate
            };
        }

        public static ArchivedPendingPaymentValidationResult Archive(this PendingPaymentValidationResult validationResult)
        {
            return new ArchivedPendingPaymentValidationResult
            {
                PendingPaymentValidationResultId = validationResult.Id,
                PendingPaymentId = validationResult.PendingPaymentId,
                PaymentYear = validationResult.PaymentYear,
                PeriodNumber = validationResult.PeriodNumber,
                CreatedDateUtc = validationResult.CreatedDateUtc,
                Step = validationResult.Step,
                Result = validationResult.Result,
            };
        }
    }
}
