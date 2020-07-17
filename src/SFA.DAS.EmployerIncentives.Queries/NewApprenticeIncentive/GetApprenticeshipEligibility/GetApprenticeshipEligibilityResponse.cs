namespace SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetApprenticeshipEligibility
{
    public class GetApprenticeshipEligibilityResponse
    {
        public bool IsEligible { get; }

        public GetApprenticeshipEligibilityResponse(bool isEligible)
        {
            IsEligible = isEligible;
        }
    }
}
