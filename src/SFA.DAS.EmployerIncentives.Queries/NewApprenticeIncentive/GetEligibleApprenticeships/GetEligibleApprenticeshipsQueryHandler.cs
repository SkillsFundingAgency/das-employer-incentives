using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Domain.Services;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Queries.Extensions;

namespace SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetEligibleApprenticeships
{
    public class GetEligibleApprenticeshipsQueryHandler : IQueryHandler<GetEligibleApprenticeshipsRequest, GetEligibleApprenticeshipsResponse>
    {
        private INewApprenticeIncentiveEligibilityService _eligibilityService;

        public GetEligibleApprenticeshipsQueryHandler(INewApprenticeIncentiveEligibilityService eligibilityService)
        {
            _eligibilityService = eligibilityService;
        }

        public async Task<GetEligibleApprenticeshipsResponse> Handle(GetEligibleApprenticeshipsRequest query, CancellationToken cancellationToken = default)
        {
            var apprenticeships = query.Apprenticeships.Select(x => x.ToApprenticeship());

            var eligibleApprenticeships = _eligibilityService.GetEligibileApprenticeships(apprenticeships);

            return new GetEligibleApprenticeshipsResponse(eligibleApprenticeships.Select(x => x.ToApprenticeshipDto()));
        }
    }
}
