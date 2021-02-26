using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.Archive
{
    [Dapper.Contrib.Extensions.Table("archive.PendingPaymentValidationResult")]
    [Table("PendingPaymentValidationResult", Schema = "archive")]
    public partial class PendingPaymentValidationResult
    {
        [Dapper.Contrib.Extensions.ExplicitKey]
        public Guid PendingPaymentValidationResultId { get; set; }
        public Guid PendingPaymentId { get; set; }
        public string Step { get; set; }
        public bool Result { get; set; }
        public byte PeriodNumber { get; set; }
        public short PaymentYear { get; set; }
        public DateTime CreatedDateUtc { get; set; }        
        public DateTime ArchiveDateUTC { get; set; }
    }
}
