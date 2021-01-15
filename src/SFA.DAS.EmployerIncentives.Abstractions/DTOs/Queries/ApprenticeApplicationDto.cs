using System;

namespace SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries
{
    public class ApprenticeApplicationDto
    {
        public long AccountId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long ULN { get; set; }
        public DateTime ApplicationDate { get; set; }
        public decimal TotalIncentiveAmount { get; set; }
        public string SubmittedByEmail { get; set; }
        public string CourseName { get; set; }
    }
}
