using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives
{
    public class PendingPayment : Entity<Guid, PendingPaymentModel>
    {
        public DateTime DatePayable => Model.DatePayable;
        public decimal Amount => Model.AmountInPence > 0 ? Model.AmountInPence/100 : 0;
        public Account Account => Model.Account;

        internal static PendingPayment New(
            Guid id, 
            Account account, 
            Guid apprenticeshipIncentiveId, 
            decimal amount,
            DateTime datePayable,
            DateTime dateCalculated
            )
        {
            return new PendingPayment(new PendingPaymentModel { 
                Id = id,
                Account = account,
                ApprenticeshipIncentiveId = apprenticeshipIncentiveId,
                AmountInPence = (long)(amount * 100),
                DateCalculated = dateCalculated,
                DatePayable = datePayable
                },
                true);
            }

        internal static PendingPayment Get(PendingPaymentModel model)
        {
            return new PendingPayment(model);
        }

        private PendingPayment(PendingPaymentModel model, bool isNew = false) : base(model.Id, model, isNew)
        {
        }
    }
}
