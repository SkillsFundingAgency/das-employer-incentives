using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;

namespace SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetEligibleApprenticeships
{
    public class GetEligibleApprenticeshipsRequest : IQuery
    {
        public IEnumerable<ApprenticeshipDto> Apprenticeships { get; }

        public GetEligibleApprenticeshipsRequest(IEnumerable<ApprenticeshipDto> apprenticeships)
        {
            Apprenticeships = apprenticeships;
        }
    }
}
