using System;
using System.ComponentModel.DataAnnotations.Schema;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models
{
    [Dapper.Contrib.Extensions.Table("incentives.EmploymentCheck")]
    [Table("EmploymentCheck", Schema = "incentives")]
    public partial class EmploymentCheck
    {
        [Dapper.Contrib.Extensions.ExplicitKey]
        public Guid Id { get; set; }
        public Guid ApprenticeshipIncentiveId { get; set; }
        public EmploymentCheckType CheckType { get; set; }
        public DateTime MinimumDate { get; set; }
        public DateTime MaximumDate { get; set; }
        public Guid CorrelationId { get; set; }
        public bool? Result { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime? ResultDateTime { get; set; }
    }
}
