namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.ApprenticeshipIncentiveHasPossibleChangeOrCircs
{
    public class ApprenticeshipIncentiveHasPossibleChangeOrCircsResponse
    {
        public bool HasPossibleChangeOfCircumstances { get; }

        public ApprenticeshipIncentiveHasPossibleChangeOrCircsResponse(bool hasPossibleChangeOfCircumstances)
        {
            HasPossibleChangeOfCircumstances = hasPossibleChangeOfCircumstances;
        }
    }
}
