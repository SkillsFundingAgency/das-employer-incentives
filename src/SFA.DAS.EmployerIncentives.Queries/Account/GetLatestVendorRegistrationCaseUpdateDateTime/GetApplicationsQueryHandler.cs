using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Data;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Queries.Account.GetLatestVendorRegistrationCaseUpdateDateTime
{
    public class GetLatestVendorRegistrationCaseUpdateDateTimeQueryHandler : IQueryHandler<GetLatestVendorRegistrationCaseUpdateDateTimeRequest, GetLatestVendorRegistrationCaseUpdateDateTimeResponse>
    {
        private readonly IAccountDataRepository _repository;

        public GetLatestVendorRegistrationCaseUpdateDateTimeQueryHandler(IAccountDataRepository repository)
        {
            _repository = repository;
        }

        public async Task<GetLatestVendorRegistrationCaseUpdateDateTimeResponse> Handle(GetLatestVendorRegistrationCaseUpdateDateTimeRequest query, CancellationToken cancellationToken = default)
        {
            return new GetLatestVendorRegistrationCaseUpdateDateTimeResponse
            {
                LastUpdateDateTime = await _repository.GetLatestVendorRegistrationCaseUpdateDateTime()
            };
        }
    }
}
