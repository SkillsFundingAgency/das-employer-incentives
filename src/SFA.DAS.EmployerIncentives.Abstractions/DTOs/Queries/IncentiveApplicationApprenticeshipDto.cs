using System;

namespace SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries
{
    public class IncentiveApplicationApprenticeshipDto
    {
        public Guid Id { get; set; }
        public long ApprenticeshipId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal TotalIncentiveAmount { get; set; }
        public long Uln { get; set; }
    }
}
