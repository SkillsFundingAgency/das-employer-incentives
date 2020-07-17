using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;

namespace SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetEligibleApprenticeships
{
    public class GetEligibleApprenticeshipsResponse
    {
        public IEnumerable<ApprenticeshipDto> EligibleApprenticeships { get; }

        public GetEligibleApprenticeshipsResponse(IEnumerable<ApprenticeshipDto> eligibleApprenticeships)
        {
            EligibleApprenticeships = eligibleApprenticeships;
        }
    }
}
