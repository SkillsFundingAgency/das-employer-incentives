using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;

namespace SFA.DAS.EmployerIncentives.Data.Models
{
    [Dapper.Contrib.Extensions.Table("IncentiveApplicationApprenticeship")]
    [Table("IncentiveApplicationApprenticeship")]
    public partial class IncentiveApplicationApprenticeship
    {
        [Dapper.Contrib.Extensions.ExplicitKey]
        public Guid Id { get; set; }
        public Guid IncentiveApplicationId { get; set; }
        public long ApprenticeshipId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public long Uln { get; set; }
        public DateTime PlannedStartDate { get; set; }
        public ApprenticeshipEmployerType ApprenticeshipEmployerTypeOnApproval { get; set; }
        public decimal TotalIncentiveAmount { get; set; }
        public bool EarningsCalculated { get; set; }
        [Dapper.Contrib.Extensions.Write(false)]
        public ICollection<ApprenticeshipIncentive> ApprenticeshipIncentives { get; set; }

        public IncentiveApplicationApprenticeship()
        {
            ApprenticeshipIncentives = new List<ApprenticeshipIncentive>();
        }
    }
}
