using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using System;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives
{
    public class ClawbackPayment : Entity<Guid, ClawbackPaymentModel>
    {
        public Account Account => Model.Account;

        internal static ClawbackPayment New(
            Guid id,
            Account account,
            Guid apprenticeshipIncentiveId,
            Guid pendingPaymentId,
            decimal amount,
            DateTime createdDate,
            SubnominalCode subnominalCode,
            Guid paymentId
        )
        {
            return new ClawbackPayment(new ClawbackPaymentModel
            {
                    Id = id,
                    Account = account,
                    ApprenticeshipIncentiveId = apprenticeshipIncentiveId,
                    PendingPaymentId = pendingPaymentId,
                    Amount = amount,
                    CreatedDate = createdDate,
                    SubnominalCode = subnominalCode,
                    PaymentId = paymentId
                },
                true);
        }

        internal static ClawbackPayment Get(ClawbackPaymentModel model)
        {
            return new ClawbackPayment(model);
        }

        private ClawbackPayment(ClawbackPaymentModel model, bool isNew = false) : base(model.Id, model, isNew)
        {
        }
    }
}
