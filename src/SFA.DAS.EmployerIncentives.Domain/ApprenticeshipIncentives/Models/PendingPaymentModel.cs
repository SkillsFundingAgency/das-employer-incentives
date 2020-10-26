using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models
{
    public class PendingPaymentModel : IEntityModel<Guid>
    {
        public Guid Id { get; set; }
        public Guid ApprenticeshipIncentiveId { get; set; }
        public Account Account { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public DateTime CalculatedDate { get; set; }
        public DateTime? PaymentMadeDate { get; set; }
        public byte? PaymentPeriod { get; set; }
        public short? PaymentYear { get; set; }
        public ICollection<PendingPaymentValidationResultModel> PendingPaymentValidationResultModels { get; set; }

        public PendingPaymentModel()
        {
            PendingPaymentValidationResultModels = new List<PendingPaymentValidationResultModel>();
        }
    }
}
