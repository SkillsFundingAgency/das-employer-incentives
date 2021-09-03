using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetApplication
{
    public class GetApplicationQueryHandler : IQueryHandler<GetApplicationRequest, GetApplicationResponse>
    {
        private readonly IQueryRepository<IncentiveApplicationDto> _applicationQueryRepository;
        private readonly IQueryRepository<LegalEntityDto> _legalEntityQueryRepository;
        private readonly IIncentivePaymentProfilesService _incentivePaymentProfilesService;


        public GetApplicationQueryHandler(
            IQueryRepository<IncentiveApplicationDto> applicationQueryRepository,
            IQueryRepository<LegalEntityDto> legalEntityQueryRepository,
            IIncentivePaymentProfilesService incentivePaymentProfilesService)
        {
            _applicationQueryRepository = applicationQueryRepository;
            _legalEntityQueryRepository = legalEntityQueryRepository;
            _incentivePaymentProfilesService = incentivePaymentProfilesService;
        }

        public async Task<GetApplicationResponse> Handle(GetApplicationRequest query, CancellationToken cancellationToken = default)
        {
            var application = await _applicationQueryRepository.Get(app => app.Id == query.ApplicationId && app.AccountId == query.AccountId);
            application.LegalEntity = await _legalEntityQueryRepository.Get(x => x.AccountLegalEntityId == application.AccountLegalEntityId);

            application.NewAgreementRequired = Incentive.IsNewAgreementRequired(application, await _incentivePaymentProfilesService.Get());

            var response = new GetApplicationResponse(application);

            return response;
        }
    }
}
