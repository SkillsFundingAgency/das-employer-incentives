using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Domain.Services;
using SFA.DAS.EmployerIncentives.Queries.Extensions;

namespace SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetApprenticeshipEligibility
{
    public class GetApprenticeshipEligibilityQueryHandler : IQueryHandler<GetApprenticeshipEligibilityRequest, GetApprenticeshipEligibilityResponse>
    {
        private readonly INewApprenticeIncentiveEligibilityService _eligibilityService;
        private readonly IUlnQueryRepository _queryRepository;

        public GetApprenticeshipEligibilityQueryHandler(INewApprenticeIncentiveEligibilityService eligibilityService, IUlnQueryRepository queryRepository)
        {
            _eligibilityService = eligibilityService;
            _queryRepository = queryRepository;
        }

        public async Task<GetApprenticeshipEligibilityResponse> Handle(GetApprenticeshipEligibilityRequest query, CancellationToken cancellationToken = default)
        {
            var apprenticeship = query.Apprenticeship.ToApprenticeship();
            var ulnUsed = await _queryRepository.UlnAlreadyOnSubmittedIncentiveApplication(query.Apprenticeship.UniqueLearnerNumber);

            var isEligible = !ulnUsed && _eligibilityService.IsApprenticeshipEligible(apprenticeship);

            return new GetApprenticeshipEligibilityResponse(isEligible);
        }
    }
}
