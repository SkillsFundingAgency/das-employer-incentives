using System;

namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public class SubmitIncentiveApplicationRequest
    {
        public Guid IncentiveApplicationId { get; set; }
        public long AccountId { get; set; }
        public DateTime DateSubmitted { get; set; }
        public string SubmittedByEmail { get; set; }
    }
}
