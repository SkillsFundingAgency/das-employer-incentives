using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models
{
    [Dapper.Contrib.Extensions.Table("audit.VendorBlockAudit")]
    [Table("VendorBlockAudit", Schema = "audit")]
    public partial class VendorBlockAudit
    {
        [Dapper.Contrib.Extensions.ExplicitKey]
        public Guid Id { get; set; }
        public string VrfVendorId { get; set; }
        public DateTime VendorBlockEndDate { get; set; }
        public string ServiceRequestTaskId { get; set; }
        public string ServiceRequestDecisionReference { get; set; }
        public DateTime? ServiceRequestCreatedDate { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }
}
