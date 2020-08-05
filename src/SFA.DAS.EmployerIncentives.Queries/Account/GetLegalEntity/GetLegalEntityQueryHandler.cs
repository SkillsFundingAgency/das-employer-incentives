using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Data;

namespace SFA.DAS.EmployerIncentives.Queries.Account.GetLegalEntity
{
    public class GetLegalEntityQueryHandler : IQueryHandler<GetLegalEntityRequest, GetLegalEntityResponse>
    {
        private readonly IQueryRepository<LegalEntityDto> _repository;

        public GetLegalEntityQueryHandler(IQueryRepository<LegalEntityDto> repository)
        {
            _repository = repository;
        }

        public async Task<GetLegalEntityResponse> Handle(GetLegalEntityRequest query, CancellationToken cancellationToken = default)
        {
            var legalEntity = await _repository.Get(account => account.AccountLegalEntityId == query.AccountLegalEntityId);

            var response = new GetLegalEntityResponse
            {
                LegalEntity = legalEntity
            };

            return response;
        }
    }
}
