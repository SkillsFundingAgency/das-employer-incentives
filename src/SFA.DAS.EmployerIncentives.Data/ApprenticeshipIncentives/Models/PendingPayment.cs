using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models
{
    [Table("PendingPayment")]
    public partial class PendingPayment
    {
        public Guid Id { get; set; }
        public long AccountId { get; set; }
        public Guid ApprenticeshipIncentiveId { get; set; }
        public long AmountPayablePence { get; set; }
        public DateTime DatePayable { get; set; }
        public DateTime DateCalculated { get; set; }
        public DateTime? DatePaymentMade { get; set; }
    }
}
