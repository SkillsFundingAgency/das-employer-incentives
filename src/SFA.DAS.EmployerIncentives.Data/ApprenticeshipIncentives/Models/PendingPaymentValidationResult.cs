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
        public string ValidationStep { get; set; }
        public bool ValidationResult { get; set; }
        public short CollectionPeriodMonth { get; set; }
        public short CollectionPeriodYear { get; set; }
        public DateTime CollectionDateUTC { get; set; }
    }
}
