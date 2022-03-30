using System;
using System.ComponentModel.DataAnnotations.Schema;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models
{
    [Dapper.Contrib.Extensions.Table("incentives.ApprenticeApplications")]
    [Table("ApprenticeApplications", Schema = "incentives")]
    public class ApprenticeApplication
    {
        public Guid Id { get; set; }
        public long AccountId { get; set; }
        public long AccountLegalEntityId { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public IncentiveStatus Status { get; set; }
        public int? MinimumAgreementVersion { get; set; }
        public DateTime SubmittedDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long ULN { get; set; }
        public string LegalEntityName { get; set; }
        public int? SignedAgreementVersion { get; set; }
        public string SubmittedByEmail { get; set; }
        public string CourseName { get; set; }
        public bool PausePayments { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public WithdrawnBy? WithdrawnBy { get; set; }
        public decimal? FirstPendingPaymentAmount { get; set; }
        public DateTime? FirstPendingPaymentDueDate { get; set; }
        public decimal? SecondPendingPaymentAmount { get; set; }
        public DateTime? SecondPendingPaymentDueDate { get; set; }
        public decimal? FirstPaymentAmount { get; set; }
        public DateTime? FirstPaymentDate { get; set; }
        public DateTime? FirstPaymentCalculatedDate { get; set; }
        public decimal? SecondPaymentAmount { get; set; }
        public DateTime? SecondPaymentDate { get; set; }
        public DateTime? SecondPaymentCalculatedDate { get; set; }
        public decimal? FirstClawbackAmount { get; set; }
        public DateTime? FirstClawbackCreated { get; set; }
        public DateTime? FirstClawbackSent { get; set; }
        public decimal? SecondClawbackAmount { get; set; }
        public DateTime? SecondClawbackCreated { get; set; }
        public DateTime? SecondClawbackSent { get; set; }
        public bool? LearningFound { get; set; }
        public bool? HasDataLock { get; set; }
        public bool? InLearning { get; set; }
        public bool? FirstEmploymentCheckValidation { get; set; }
        public bool? FirstEmploymentCheckResult { get; set; }
        public string FirstEmploymentCheckErrorType { get; set; }
        public bool? SecondEmploymentCheckValidation { get; set; }
        public bool? SecondEmploymentCheckResult { get; set; }
        public string SecondEmploymentCheckErrorType { get; set; }
    }
}
