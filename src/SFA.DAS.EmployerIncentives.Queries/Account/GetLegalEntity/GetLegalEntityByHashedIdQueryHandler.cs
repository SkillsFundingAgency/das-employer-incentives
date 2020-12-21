using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Data;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Queries.Account.GetLegalEntity
{
    public class GetLegalEntityByHashedIdQueryHandler : IQueryHandler<GetLegalEntityByHashedIdRequest, GetLegalEntityResponse>
    {
        private readonly IQueryRepository<LegalEntityDto> _repository;

        public GetLegalEntityByHashedIdQueryHandler(IQueryRepository<LegalEntityDto> repository)
        {
            _repository = repository;
        }

        public async Task<GetLegalEntityResponse> Handle(GetLegalEntityByHashedIdRequest query, CancellationToken cancellationToken = default)
        {
            var legalEntity = await _repository.Get(x => x.HashedLegalEntityId == query.HashedLegalEntityId);
            if (legalEntity == null)
            {
                return null;
            }

            return new GetLegalEntityResponse { LegalEntity = legalEntity };
        }
    }
}
