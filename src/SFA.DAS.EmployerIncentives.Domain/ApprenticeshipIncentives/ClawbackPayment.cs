using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives
{
    public class ClawbackPayment : Entity<Guid, ClawbackPaymentModel>
    {
        public Account Account => Model.Account;
        public bool Sent => Model.DateClawbackSent.HasValue;
        public string VrfVendorId => Model.VrfVendorId;

        internal static ClawbackPayment New(
            Guid id,
            Account account,
            Guid apprenticeshipIncentiveId,
            Guid pendingPaymentId,
            decimal amount,
            DateTime createdDate,
            SubnominalCode subnominalCode,
            Guid paymentId,
            string vrfVendorId
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
                PaymentId = paymentId,
                VrfVendorId = vrfVendorId
            },
                true);
        }

        internal static ClawbackPayment Get(ClawbackPaymentModel model)
        {
            return new ClawbackPayment(model);
        }

        public void SetPaymentPeriod(CollectionPeriod period)
        {
            Model.CollectionPeriod = period.PeriodNumber;
            Model.CollectionPeriodYear = period.AcademicYear;
        }

        private ClawbackPayment(ClawbackPaymentModel model, bool isNew = false) : base(model.Id, model, isNew)
        {
        }
    }
}
