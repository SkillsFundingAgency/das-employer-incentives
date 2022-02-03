using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models
{
    [Dapper.Contrib.Extensions.Table("incentives.ValidationOverride")]
    [Table("ValidationOverride", Schema = "incentives")]
    public partial class ValidationOverride
    {
        [Dapper.Contrib.Extensions.ExplicitKey]        
        public Guid Id { get; set; }
        
        public Guid ApprenticeshipIncentiveId { get; set; }        
        public string Step { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }
}
