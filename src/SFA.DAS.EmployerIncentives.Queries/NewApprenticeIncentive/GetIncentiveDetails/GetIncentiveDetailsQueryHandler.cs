using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetIncentiveDetails
{
    public class GetIncentiveDetailsQueryHandler : IQueryHandler<GetIncentiveDetailsRequest, GetIncentiveDetailsResponse>
    {
        public Task<GetIncentiveDetailsResponse> Handle(GetIncentiveDetailsRequest query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new GetIncentiveDetailsResponse(Phase2Incentive.EligibilityStartDate, Phase3Incentive.EligibilityEndDate));
        }
    }
}
