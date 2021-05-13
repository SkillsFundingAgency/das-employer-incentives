﻿using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;

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
        public long ULN { get; set; }
        public DateTime PlannedStartDate { get; set; }
        public ApprenticeshipEmployerType ApprenticeshipEmployerTypeOnApproval { get; set; }
        public decimal TotalIncentiveAmount { get; set; }
        public long? UKPRN { get; set; }
        public bool EarningsCalculated { get; set; }
        public bool WithdrawnByEmployer { get; set; }
        public bool WithdrawnByCompliance { get; set; }
        public string CourseName { get; set; }
        public DateTime? EmploymentStartDate { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public Phase Phase { get; set; }
    }
}
