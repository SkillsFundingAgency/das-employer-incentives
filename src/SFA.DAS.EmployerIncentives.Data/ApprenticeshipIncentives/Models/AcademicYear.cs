using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models
{
    [Dapper.Contrib.Extensions.Table("incentives.AcademicYear")]
    [Table("AcademicYear", Schema = "incentives")]
    public partial class AcademicYear
    {
        [Dapper.Contrib.Extensions.ExplicitKey]
        public string Id { get; set; }
        public DateTime EndDate { get; set; }
    }
}
