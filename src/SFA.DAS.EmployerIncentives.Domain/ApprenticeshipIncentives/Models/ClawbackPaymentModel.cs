using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models
{
    public class ClawbackPaymentModel : IEntityModel<Guid>
    {
        public Guid Id { get; set; }
        public Guid ApprenticeshipIncentiveId { get; set; }
        public Guid PendingPaymentId { get; set; }
        public Account Account { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedDate { get; set; }
        public SubnominalCode SubnominalCode { get; set; }
        public Guid PaymentId { get; set; }
    }
}
