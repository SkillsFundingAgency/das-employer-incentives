using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Data.Models
{
    [Table("IncentiveApplication")]
    public partial class IncentiveApplication
    {
        public IncentiveApplication()
        {
            Apprenticeships = new List<IncentiveApplicationApprenticeship>();
        }

        public Guid Id { get; set; }
        public long AccountId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public DateTime DateCreated { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public IncentiveApplicationStatus Status { get; set; }
        public DateTime? DateSubmitted { get; set; }
        public string SubmittedByEmail { get; set; }
        public ICollection<IncentiveApplicationApprenticeship> Apprenticeships { get; set; }
    }
}
