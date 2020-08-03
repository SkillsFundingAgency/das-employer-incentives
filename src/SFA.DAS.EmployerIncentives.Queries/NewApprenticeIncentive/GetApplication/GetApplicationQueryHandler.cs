using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Data;

namespace SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetApplication
{
    public class GetApplicationQueryHandler : IQueryHandler<GetApplicationRequest, GetApplicationResponse>
    {
        private IQueryRepository<IncentiveApplicationDto> _queryRepository;

        public GetApplicationQueryHandler(IQueryRepository<IncentiveApplicationDto> queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<GetApplicationResponse> Handle(GetApplicationRequest query, CancellationToken cancellationToken = default)
        {
            var application = await _queryRepository.Get(application => application.Id == query.ApplicationId);

            var response = new GetApplicationResponse(application);

            return response;
        }
    }
}
