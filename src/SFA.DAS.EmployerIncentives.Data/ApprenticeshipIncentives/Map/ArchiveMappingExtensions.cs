using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Map
{
    public static class ArchiveMappingExtensions
    {
        internal static Models.Archive.PendingPayment Map(this PendingPaymentModel model)
        {
            return new Models.Archive.PendingPayment
            {
                PendingPaymentId = model.Id,
                AccountId = model.Account.Id,
                AccountLegalEntityId = model.Account.AccountLegalEntityId,
                ApprenticeshipIncentiveId = model.ApprenticeshipIncentiveId,
                Amount = model.Amount,
                DueDate = model.DueDate,
                CalculatedDate = model.CalculatedDate,
                PeriodNumber = model.AcademicPeriod.PeriodNumber,
                PaymentYear = model.AcademicPeriod.AcademicYear,
                PaymentMadeDate = model.PaymentMadeDate,
                EarningType = model.EarningType,
                ClawedBack = model.ClawedBack,
                ArchiveDateUTC = DateTime.UtcNow
            };
        }

        internal static Models.Archive.Payment Map(this PaymentModel model)
        {
            return new Models.Archive.Payment
            {
                PaymentId = model.Id,
                ApprenticeshipIncentiveId = model.ApprenticeshipIncentiveId,
                PendingPaymentId = model.PendingPaymentId,
                AccountId = model.Account.Id,
                AccountLegalEntityId = model.Account.AccountLegalEntityId,
                Amount = model.Amount,
                CalculatedDate = model.CalculatedDate,
                PaidDate = model.PaidDate,
                SubnominalCode = model.SubnominalCode,
                PaymentPeriod = model.PaymentPeriod,
                PaymentYear = model.PaymentYear,
                ArchiveDateUTC = DateTime.UtcNow
            };
        }

        internal static ICollection<Models.Archive.PendingPaymentValidationResult> ArchiveMap(this ICollection<PendingPaymentValidationResultModel> models, Guid pendingPaymentId)
        {
            return models.Select(x => x.ArchiveMap(pendingPaymentId)).ToList();
        }

        internal static Models.Archive.PendingPaymentValidationResult ArchiveMap(this PendingPaymentValidationResultModel model, Guid pendingPaymentId)
        {
            return new Models.Archive.PendingPaymentValidationResult
            {
                PendingPaymentValidationResultId = model.Id,
                PendingPaymentId = pendingPaymentId,
                Step = model.Step,
                Result = model.Result,
                PeriodNumber = model.AcademicPeriod.PeriodNumber,
                PaymentYear = model.AcademicPeriod.AcademicYear,
                CreatedDateUtc = model.CreatedDateUtc,
                ArchiveDateUTC = DateTime.UtcNow
            };
        }
    }
}
