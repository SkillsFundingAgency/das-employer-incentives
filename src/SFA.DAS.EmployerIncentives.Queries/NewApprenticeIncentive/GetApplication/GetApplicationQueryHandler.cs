using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Data;

namespace SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetApplication
{
    public class GetApplicationQueryHandler : IQueryHandler<GetApplicationRequest, GetApplicationResponse>
    {
        private readonly IQueryRepository<IncentiveApplicationDto> _applicationQueryRepository;

        public GetApplicationQueryHandler(IQueryRepository<IncentiveApplicationDto> applicationQueryRepository)
        {
            _applicationQueryRepository = applicationQueryRepository;
        }

        public async Task<GetApplicationResponse> Handle(GetApplicationRequest query, CancellationToken cancellationToken = default)
        {
            var application = await _applicationQueryRepository.Get(app => app.Id == query.ApplicationId && app.AccountId == query.AccountId);

            var response = new GetApplicationResponse(application);

            return response;
        }
    }
}
