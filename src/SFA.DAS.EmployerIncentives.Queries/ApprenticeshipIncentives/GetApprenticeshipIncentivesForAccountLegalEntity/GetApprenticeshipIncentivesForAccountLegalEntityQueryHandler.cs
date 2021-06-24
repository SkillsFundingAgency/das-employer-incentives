using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetApprenticeshipIncentivesForAccountLegalEntity
{
    public class GetApprenticeshipIncentivesForAccountLegalEntityQueryHandler : IQueryHandler<GetApprenticeshipIncentivesForAccountLegalEntityRequest, GetApprenticeshipIncentivesForAccountLegalEntityResponse>
    {
        private readonly IApprenticeshipIncentiveQueryRepository _queryRepository;

        public GetApprenticeshipIncentivesForAccountLegalEntityQueryHandler(IApprenticeshipIncentiveQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<GetApprenticeshipIncentivesForAccountLegalEntityResponse> Handle(GetApprenticeshipIncentivesForAccountLegalEntityRequest query, CancellationToken cancellationToken = default)
        {
            var apprenticeshipIncentives = await _queryRepository.GetWithdrawable(query.AccountId, query.AccountLegalEntityId);

            var response = new GetApprenticeshipIncentivesForAccountLegalEntityResponse(apprenticeshipIncentives);

            return response;
        }
    }
}
