using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Data;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;

namespace SFA.DAS.EmployerIncentives.Queries.Account
{
    public class GetLegalEntitiesQueryHandler : IQueryHandler<GetLegalEntitiesRequest, GetLegalEntitiesResponse>
    {
        private readonly IQueryRepository<LegalEntityDto> _repository;

        public GetLegalEntitiesQueryHandler(IQueryRepository<LegalEntityDto> repository)
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
