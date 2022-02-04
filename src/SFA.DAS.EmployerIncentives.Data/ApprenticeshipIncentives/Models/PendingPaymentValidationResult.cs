using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models
{
    [Dapper.Contrib.Extensions.Table("incentives.PendingPaymentValidationResult")]
    [Table("PendingPaymentValidationResult", Schema = "incentives")]
    public partial class PendingPaymentValidationResult
    {
        [Dapper.Contrib.Extensions.ExplicitKey]
        public Guid Id { get; set; }
        public Guid PendingPaymentId { get; set; }
        public string Step { get; set; }
        [Column("Result")]
        public bool ValidationResult { get; set; }
        public bool? OverrideResult { get; set; }        
        public byte PeriodNumber { get; set; }
        public short PaymentYear { get; set; }
        public DateTime CreatedDateUtc { get; set; }
    }
}
