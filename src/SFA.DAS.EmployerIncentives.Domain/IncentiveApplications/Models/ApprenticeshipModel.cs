using System;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models
{
    public class ApprenticeshipModel : IEntityModel<Guid>
    {
        public Guid Id { get; set; }
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
        public Phase Phase { get; set; }
    }
}
