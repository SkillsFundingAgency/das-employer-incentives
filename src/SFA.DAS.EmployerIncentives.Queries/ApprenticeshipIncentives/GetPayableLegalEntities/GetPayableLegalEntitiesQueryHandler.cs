using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetPayableLegalEntities
{
    public class GetPayableLegalEntitiesQueryHandler : IQueryHandler<GetPayableLegalEntitiesRequest, GetPayableLegalEntitiesResponse>
    {
        private IPayableLegalEntityQueryRepository _queryRepository;

        public GetPayableLegalEntitiesQueryHandler(IPayableLegalEntityQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<GetPayableLegalEntitiesResponse> Handle(GetPayableLegalEntitiesRequest query, CancellationToken cancellationToken = default)
        {
            var payableLegalEntities = await _queryRepository.GetList(query.CollectionPeriodYear, query.CollectionPeriodMonth);

            var response = new GetPayableLegalEntitiesResponse(payableLegalEntities);

            return response;
        }
    }
}
