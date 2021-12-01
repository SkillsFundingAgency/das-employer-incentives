using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Map
{
    public static class DomainExtensions
    {
        public static IEnumerable<PendingPayment> Map(this IEnumerable<PendingPaymentModel> models)
        {
            return models.Select(q => q.Map());
        }

        public static PendingPayment Map(this PendingPaymentModel model)
        {
            return PendingPayment.Get(model);
        }

        public static IEnumerable<Payment> Map(this IEnumerable<PaymentModel> models)
		{
            return models.Select(q => q.Map());
        }

        public static IEnumerable<PendingPaymentValidationResult> Map(this IEnumerable<PendingPaymentValidationResultModel> models)
        {
            return models.Select(q => q.Map());
        }

        public static Payment Map(this PaymentModel model)
        {
            return Payment.Get(model);
        }

        public static PendingPaymentValidationResult Map(this PendingPaymentValidationResultModel model)
        {
            return PendingPaymentValidationResult.Get(model);
        }

        public static LegalEntity Map(this LegalEntityDto legalEntityDto)
        {
            return new LegalEntity(
                new Account(legalEntityDto.AccountId, legalEntityDto.AccountLegalEntityId),
                legalEntityDto.LegalEntityId,
                legalEntityDto.LegalEntityName,
                legalEntityDto.VrfVendorId,
                legalEntityDto.VrfCaseStatus,
                legalEntityDto.HashedLegalEntityId);
        }

        public static IEnumerable<ClawbackPayment> Map(this IEnumerable<ClawbackPaymentModel> models)
        {
            return models.Select(q => q.Map());
        }

        public static ClawbackPayment Map(this ClawbackPaymentModel model)
        {
            return ClawbackPayment.Get(model);
        }

        public static IEnumerable<EmploymentCheck> Map(this IEnumerable<EmploymentCheckModel> models)
        {
            return models.Select(x => x.Map());
        }

        public static EmploymentCheck Map(this EmploymentCheckModel model)
        {
            return EmploymentCheck.Get(model);
        }
    }
}
