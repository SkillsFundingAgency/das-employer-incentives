using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Data;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Queries.Account.GetVendorId
{
    public class GetVendorIdQueryHandler : IQueryHandler<GetVendorIdRequest, GetVendorIdResponse>
    {
        private readonly IQueryRepository<LegalEntityDto> _repository;

        public GetVendorIdQueryHandler(IQueryRepository<LegalEntityDto> repository)
        {
            _repository = repository;
        }

        public async Task<GetVendorIdResponse> Handle(GetVendorIdRequest query, CancellationToken cancellationToken = default)
        {
            var legalEntity = await _repository.Get(x => x.LegalEntityId == query.LegalEntityId);
            if (legalEntity == null)
            {
                return null;
            }

            return new GetVendorIdResponse { VendorId = legalEntity.VrfVendorId };
        }
    }
}
