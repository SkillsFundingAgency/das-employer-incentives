using System;

namespace SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetApprenticeshipEligibility
{

    public class EligibleApprenticeshipCheckDetails
    {
        public long Uln { get; set; }
        public DateTime StartDate { get; set; }
        public bool IsApproved { get; set; }
    }


}
