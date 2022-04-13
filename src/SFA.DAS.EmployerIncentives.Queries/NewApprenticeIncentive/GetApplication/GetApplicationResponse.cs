using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries;

namespace SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetApplication
{
    public class GetApplicationResponse
    {
        public IncentiveApplication Application { get; }

        public GetApplicationResponse(IncentiveApplication application)
        {
            Application = application;
        }
    }
}
