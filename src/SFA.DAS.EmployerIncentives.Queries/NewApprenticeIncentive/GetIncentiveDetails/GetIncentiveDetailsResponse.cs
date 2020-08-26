using System;

namespace SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetIncentiveDetails
{
    public class GetIncentiveDetailsResponse
    {
        public DateTime EligibilityStartDate { get; }
        public DateTime EligibilityEndDate { get; }

        public GetIncentiveDetailsResponse(DateTime eligibilityStartDate, DateTime eligibilityEndDate)
        {
            EligibilityStartDate = eligibilityStartDate;
            EligibilityEndDate = eligibilityEndDate;
        }
    }
}
