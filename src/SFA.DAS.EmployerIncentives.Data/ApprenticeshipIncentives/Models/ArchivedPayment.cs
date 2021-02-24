using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models
{
    [Dapper.Contrib.Extensions.Table("archive.Payment")]
    [System.ComponentModel.DataAnnotations.Schema.Table("Payment", Schema = "archive")]
    public partial class ArchivedPayment
    {
        [Dapper.Contrib.Extensions.ExplicitKey]
        [Key]
        public Guid PaymentId { get; set; }
        public Guid ApprenticeshipIncentiveId { get; set; }
        public Guid PendingPaymentId { get; set; }
        public long AccountId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public decimal Amount { get; set; }
        public DateTime CalculatedDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public SubnominalCode SubnominalCode { get; set; }
        public byte PaymentPeriod { get; set; }
        public short PaymentYear { get; set; }
        public DateTime ArchivedDateUtc { get; set; } = DateTime.UtcNow;
    }
}
