using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;

namespace SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetApplication
{
    public class GetApplicationResponse
    {
        public IncentiveApplicationDto Application { get; }

        public bool NewAgreementRequired { get; }

        public GetApplicationResponse(IncentiveApplicationDto application, bool newAgreementRequired)
        {
            Application = application;
            NewAgreementRequired = newAgreementRequired;
        }
    }
}
