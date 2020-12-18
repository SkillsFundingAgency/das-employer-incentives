using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetApprenticeshipIncentives
{
    public class GetApprenticeshipIncentivesQueryHandler : IQueryHandler<GetApprenticeshipIncentivesRequest, GetApprenticeshipIncentivesResponse>
    {
        private readonly IApprenticeshipIncentiveQueryRepository _queryRepository;

        public GetApprenticeshipIncentivesQueryHandler(IApprenticeshipIncentiveQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<GetApprenticeshipIncentivesResponse> Handle(GetApprenticeshipIncentivesRequest query, CancellationToken cancellationToken = default)
        {
            var apprenticeshipIncentives = await _queryRepository.GetList();

            var response = new GetApprenticeshipIncentivesResponse(apprenticeshipIncentives);

            return response;
        }
    }
}
