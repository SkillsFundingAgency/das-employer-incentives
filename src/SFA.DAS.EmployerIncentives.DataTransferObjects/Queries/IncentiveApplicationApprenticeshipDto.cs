using System;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.DataTransferObjects.Queries
{
    public class IncentiveApplicationApprenticeshipDto
    {
        public Guid Id { get; set; }
        public long ApprenticeshipId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal TotalIncentiveAmount { get; set; }
        public long Uln { get; set; }
        public DateTime PlannedStartDate { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime? EmploymentStartDate { get; set; }
        public bool StartDatesAreEligible { get; set; }
        public Phase Phase { get; set; }
        
    }
}
