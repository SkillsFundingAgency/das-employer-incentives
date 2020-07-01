using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Data;

namespace SFA.DAS.EmployerIncentives.Queries.Account
{
    public class GetLegalEntitiesQueryHandler : IQueryHandler<GetLegalEntitiesRequest, GetLegalEntitiesResponse>
    {
        private readonly IReadonlyRepository<Domain.Accounts.Account> _repository;

        public GetLegalEntitiesQueryHandler(IReadonlyRepository<Domain.Accounts.Account> repository)
        {
            _repository = repository;
        }
        public async Task<GetLegalEntitiesResponse> Handle(GetLegalEntitiesRequest query, CancellationToken cancellationToken)
        {
             var accounts= await _repository.GetList(account => account.Id == query.AccountId);
             var dtos = accounts.ToLegalEntityDto();

             var response = new GetLegalEntitiesResponse
             {
                 LegalEntities = dtos
             };

             return response;
        }
    }
}
