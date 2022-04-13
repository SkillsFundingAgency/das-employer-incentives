using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.DataTransferObjects;

namespace SFA.DAS.EmployerIncentives.Queries.Account.GetLegalEntities
{
    public class GetLegalEntitiesQueryHandler : IQueryHandler<GetLegalEntitiesRequest, GetLegalEntitiesResponse>
    {
        private readonly IQueryRepository<LegalEntity> _repository;

        public GetLegalEntitiesQueryHandler(IQueryRepository<LegalEntity> repository)
        {
            _repository = repository;
        }

        public async Task<GetLegalEntitiesResponse> Handle(GetLegalEntitiesRequest query, CancellationToken cancellationToken = default)
        {
            var accounts = await _repository.GetList(account => account.AccountId == query.AccountId);

            var response = new GetLegalEntitiesResponse
            {
                LegalEntities = accounts
            };

            return response;
        }
    }
}
