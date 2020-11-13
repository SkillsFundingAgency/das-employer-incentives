using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Data;

namespace SFA.DAS.EmployerIncentives.Queries.Account.GetApplications
{
    public class GetApplicationsQueryHandler : IQueryHandler<GetApplicationsRequest, GetApplicationsResponse>
    {
        private readonly IApprenticeApplicationDataRepository _repository;

        public GetApplicationsQueryHandler(IApprenticeApplicationDataRepository repository)
        {
            _repository = repository;
        }

        public async Task<GetApplicationsResponse> Handle(GetApplicationsRequest query, CancellationToken cancellationToken = default)
        {
            var applications = await _repository.GetList(query.AccountId, query.AccountLegalEntityId);

            var response = new GetApplicationsResponse
            {
                ApprenticeApplications = applications
            };

            return response;
        }
    }
}
