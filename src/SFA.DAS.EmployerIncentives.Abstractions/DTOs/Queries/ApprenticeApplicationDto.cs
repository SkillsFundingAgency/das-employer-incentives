using System;

namespace SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries
{
    public class ApprenticeApplicationDto
    {
        public long AccountId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public Guid ApplicationId { get; set; }
        public string LegalEntityName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime ApplicationDate { get; set; }
        public decimal TotalIncentiveAmount { get; set; }
        public string Status { get; set; }
        public string SubmittedByEmail { get; set; }
    }
}
