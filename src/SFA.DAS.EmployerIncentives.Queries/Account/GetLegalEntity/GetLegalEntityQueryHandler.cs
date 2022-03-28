using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.DataTransferObjects;

namespace SFA.DAS.EmployerIncentives.Queries.Account.GetLegalEntity
{
    public class GetLegalEntityQueryHandler : IQueryHandler<GetLegalEntityRequest, GetLegalEntityResponse>
    {
        private readonly IQueryRepository<LegalEntity> _repository;

        public GetLegalEntityQueryHandler(IQueryRepository<LegalEntity> repository)
        {
            _repository = repository;
        }

        public async Task<GetLegalEntityResponse> Handle(GetLegalEntityRequest query, CancellationToken cancellationToken = default)
        {
            var legalEntity = await _repository.Get(account => account.AccountId == query.AccountId && account.AccountLegalEntityId == query.AccountLegalEntityId);

            var response = new GetLegalEntityResponse
            {
                LegalEntity = legalEntity
            };

            return response;
        }
    }
}
