using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetClawbackLegalEntities
{
    public class GetClawbackLegalEntitiesQueryHandler : IQueryHandler<GetClawbackLegalEntitiesRequest, GetClawbackLegalEntitiesResponse>
    {
        private readonly IPaymentsQueryRepository _queryRepository;

        public GetClawbackLegalEntitiesQueryHandler(IPaymentsQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<GetClawbackLegalEntitiesResponse> Handle(GetClawbackLegalEntitiesRequest query, CancellationToken cancellationToken = default)
        {
            var payableLegalEntities = await _queryRepository.GetClawbackLegalEntities(query.CollectionPeriodYear, query.CollectionPeriodMonth, query.IsSent);

            var response = new GetClawbackLegalEntitiesResponse(payableLegalEntities);

            return response;
        }
    }
}
