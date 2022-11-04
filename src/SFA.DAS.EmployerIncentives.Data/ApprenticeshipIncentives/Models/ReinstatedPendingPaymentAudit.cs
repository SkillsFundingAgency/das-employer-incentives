using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models
{
    [Dapper.Contrib.Extensions.Table("audit.ReinstatedPendingPayment")]
    [Table("ReinstatedPendingPayment", Schema = "audit")]
    public class ReinstatedPendingPaymentAudit
    {
        public Guid Id { get; set; }
        public Guid ApprenticeshipIncentiveId { get; set; }
        public Guid PendingPaymentId { get; set; }
        public string ServiceRequestTaskId { get; set; }
        public string ServiceRequestDecisionReference { get; set; }
        public DateTime? ServiceRequestCreatedDate { get; set; }
        public string Process { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }
}
