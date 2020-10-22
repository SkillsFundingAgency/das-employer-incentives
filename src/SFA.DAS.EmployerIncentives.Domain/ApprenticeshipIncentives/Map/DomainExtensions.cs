using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
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

        public static Payment Map(this PaymentModel model)
        {
            return Payment.Get(model);
        }
    }
}
