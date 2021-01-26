using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.ApprenticeshipIncentiveHasPossibleChangeOrCircs
{
    public class ApprenticeshipIncentiveHasPossibleChangeOrCircsQueryHandler : IQueryHandler<ApprenticeshipIncentiveHasPossibleChangeOrCircsRequest, ApprenticeshipIncentiveHasPossibleChangeOrCircsResponse>
    {
        private readonly IApprenticeshipIncentiveQueryRepository _queryRepository;

        public ApprenticeshipIncentiveHasPossibleChangeOrCircsQueryHandler(IApprenticeshipIncentiveQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<ApprenticeshipIncentiveHasPossibleChangeOrCircsResponse> Handle(ApprenticeshipIncentiveHasPossibleChangeOrCircsRequest query, CancellationToken cancellationToken = default)
        {
            var apprenticeshipIncentive = await _queryRepository.Get(x => x.Id == query.ApprenticeshipIncentiveId);

            var response = new ApprenticeshipIncentiveHasPossibleChangeOrCircsResponse(apprenticeshipIncentive.HasPossibleChangeOfCircumstances);

            return response;
        }
    }
}
