using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetIncentiveDetails
{
    public class GetIncentiveDetailsQueryHandler : IQueryHandler<GetIncentiveDetailsRequest, GetIncentiveDetailsResponse>
    {
#pragma warning disable 1998
        public async Task<GetIncentiveDetailsResponse> Handle(GetIncentiveDetailsRequest query, CancellationToken cancellationToken = default)
#pragma warning restore 1998
        {
            return new GetIncentiveDetailsResponse(ValueObjects.NewApprenticeIncentive.EligibilityStartDate, ValueObjects.NewApprenticeIncentive.EligibilityEndDate);
        }
    }
}
