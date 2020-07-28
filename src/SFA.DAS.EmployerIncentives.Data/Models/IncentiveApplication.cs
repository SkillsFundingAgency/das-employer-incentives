using System;
using System.ComponentModel.DataAnnotations.Schema;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Data.Models
{
    [Table("IncentiveApplication")]
    public partial class IncentiveApplication
    {
        public Guid Id { get; set; }
        public long AccountId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public DateTime DateCreated { get; set; }
        public IncentiveApplicationStatus Status { get; set; }
        public DateTime DateSubmitted { get; set; }
        public string SubmittedBy { get; set; }
    }
}
