using System;

namespace SFA.DAS.EmployerIncentives.Abstractions.DTOs
{
    public class ApprenticeshipDto
    {
        public long UniqueLearnerNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime StartDate { get; set; }
        public bool IsApproved { get; set; }
    }
}
