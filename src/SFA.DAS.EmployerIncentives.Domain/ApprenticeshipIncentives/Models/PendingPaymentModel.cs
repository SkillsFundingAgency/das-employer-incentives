using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models
{
    public class PendingPaymentModel : IEntityModel<Guid>
    {
        public Guid Id { get; set; }
        public Guid ApprenticeshipIncentiveId { get; set; }
        public Account Account { get; set; }
        public DateTime DatePayable { get; set; }
        public long AmountInPence { get; set; }
        public DateTime DateCalculated { get; set; }
    }
}
