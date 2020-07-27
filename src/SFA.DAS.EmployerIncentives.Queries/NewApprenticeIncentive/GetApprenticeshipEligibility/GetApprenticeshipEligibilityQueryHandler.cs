using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Domain.Services;
using SFA.DAS.EmployerIncentives.Queries.Extensions;

namespace SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetApprenticeshipEligibility
{
    public class GetApprenticeshipEligibilityQueryHandler : IQueryHandler<GetApprenticeshipEligibilityRequest, GetApprenticeshipEligibilityResponse>
    {
        private INewApprenticeIncentiveEligibilityService _eligibilityService;

        public GetApprenticeshipEligibilityQueryHandler(INewApprenticeIncentiveEligibilityService eligibilityService)
        {
            _eligibilityService = eligibilityService;
        }

        public async Task<GetApprenticeshipEligibilityResponse> Handle(GetApprenticeshipEligibilityRequest query, CancellationToken cancellationToken = default)
        {
            var apprenticeship = query.Apprenticeship.ToApprenticeship();

            var isEligible = _eligibilityService.IsApprenticeshipEligible(apprenticeship);

            return new GetApprenticeshipEligibilityResponse(isEligible);
        }
    }
}
