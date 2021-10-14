using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Data.IncentiveApplication;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;

namespace SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetApplication
{
    public class GetApplicationQueryHandler : IQueryHandler<GetApplicationRequest, GetApplicationResponse>
    {
        private readonly IIncentiveApplicationQueryRepository _applicationQueryRepository;
        private readonly IIncentivePaymentProfilesService _incentivePaymentProfilesService;

        public GetApplicationQueryHandler(IIncentiveApplicationQueryRepository applicationQueryRepository, IIncentivePaymentProfilesService incentivePaymentProfilesService)
        {
            _applicationQueryRepository = applicationQueryRepository;
            _incentivePaymentProfilesService = incentivePaymentProfilesService;
        }

        public async Task<GetApplicationResponse> Handle(GetApplicationRequest query, CancellationToken cancellationToken = default)
        {
            var paymentProfiles = (await _incentivePaymentProfilesService.Get()).ToList();
            var application = await _applicationQueryRepository.Get(paymentProfiles, app => app.Id == query.ApplicationId && app.AccountId == query.AccountId);

            var response = new GetApplicationResponse(application);

            return response;
        }
    }
}
