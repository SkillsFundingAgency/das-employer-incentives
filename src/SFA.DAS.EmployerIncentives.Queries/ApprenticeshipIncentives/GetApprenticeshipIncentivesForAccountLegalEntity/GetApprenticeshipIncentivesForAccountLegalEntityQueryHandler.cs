using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Enums;

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
            var apprenticeshipIncentives = await _queryRepository.GetDtoList(
                x => x.AccountId == query.AccountId 
                && x.AccountLegalEntityId == query.AccountLegalEntityId 
                && (query.IncludeWithdrawn ? x.Status == IncentiveStatus.Withdrawn : x.Status != IncentiveStatus.Withdrawn)); // don't remove the brackets

            var response = new GetApprenticeshipIncentivesForAccountLegalEntityResponse(apprenticeshipIncentives);

            return response;
        }
    }
}
