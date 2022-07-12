using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models
{
    [Dapper.Contrib.Extensions.Table("audit.RevertedPayment")]
    [Table("RevertedPayment", Schema = "audit")]
    public partial class RevertedPaymentAudit
    {
        [Dapper.Contrib.Extensions.ExplicitKey]
        public Guid Id { get; set; }
        public Guid ApprenticeshipIncentiveId { get; set; }
        public Guid PaymentId { get; set; }
        public Guid PendingPaymentId { get; set; }
        public byte PaymentPeriod { get; set; }
        public short PaymentYear { get; set; }
        public decimal Amount { get; set; }
        public DateTime CalculatedDate { get; set; }
        public DateTime PaidDate { get; set; }
        public string VrfVendorId { get; set; }
        public string ServiceRequestTaskId { get; set; }
        public string ServiceRequestDecisionReference { get; set; }
        public DateTime? ServiceRequestCreatedDate { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }
}
