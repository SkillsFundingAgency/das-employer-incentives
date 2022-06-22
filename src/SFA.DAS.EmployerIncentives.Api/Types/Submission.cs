using System;

namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public class Submission
    {
        public Guid IncentiveApplicationId { get; set; }
        public long AccountId { get; set; }
        public DateTime DateSubmitted { get; set; }
        public string SubmittedByEmail { get; set; }
        public string SubmittedByName { get; set; }
    }
}
