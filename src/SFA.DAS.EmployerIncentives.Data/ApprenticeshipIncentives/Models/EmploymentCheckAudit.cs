using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models
{
    [Dapper.Contrib.Extensions.Table("audit.EmploymentCheckAudit")]
    [Table("EmploymentCheckAudit", Schema = "audit")]
    public partial class EmploymentCheckAudit
    {
        [Dapper.Contrib.Extensions.ExplicitKey]
        public Guid Id { get; set; }
        public Guid ApprenticeshipIncentiveId { get; set; }
        public string ServiceRequestTaskId { get; set; }
        public string ServiceRequestDecisionReference { get; set; }
        public DateTime ServiceRequestCreatedDate { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }
}
