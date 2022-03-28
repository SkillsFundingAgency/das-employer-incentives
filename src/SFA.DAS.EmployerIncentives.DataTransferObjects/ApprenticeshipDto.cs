using System;

namespace SFA.DAS.EmployerIncentives.DataTransferObjects
{
    public class ApprenticeshipDto
    {
        public long UniqueLearnerNumber { get; set; }
        public DateTime StartDate { get; set; }
        public bool IsApproved { get; set; }
    }
}
