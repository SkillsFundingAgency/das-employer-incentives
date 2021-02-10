using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.EmployerIncentives.Data.Models
{
    [Dapper.Contrib.Extensions.Table("IncentiveApplicationStatusAudit")]
    [Table("IncentiveApplicationStatusAudit")]
    public partial class IncentiveApplicationStatusAudit
    {
        [Dapper.Contrib.Extensions.ExplicitKey]
        public Guid Id { get; set; }

        public Guid IncentiveApplicationApprenticeshipId { get; set; }

        [Column(TypeName = "nvarchar(20)")]
        public IncentiveApplicationStatus Process { get; set; }

        public string ServiceRequestTaskId { get; set; }
        public string ServiceRequestDecisionReference { get; set; }
        public DateTime ServiceRequestCreatedDate { get; set; }

        public DateTime CreatedDateTime { get; set; }
    }
}
