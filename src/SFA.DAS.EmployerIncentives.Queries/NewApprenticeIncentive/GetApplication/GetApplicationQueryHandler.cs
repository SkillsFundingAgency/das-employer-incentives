﻿using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetApplication
{
    public class GetApplicationQueryHandler : IQueryHandler<GetApplicationRequest, GetApplicationResponse>
    {
        private readonly IQueryRepository<IncentiveApplicationDto> _applicationQueryRepository;
        private readonly IQueryRepository<LegalEntityDto> _legalEntityQueryRepository;

        public GetApplicationQueryHandler(
            IQueryRepository<IncentiveApplicationDto> applicationQueryRepository, 
            IQueryRepository<LegalEntityDto> legalEntityQueryRepository)
        {
            _applicationQueryRepository = applicationQueryRepository;
            _legalEntityQueryRepository = legalEntityQueryRepository;
        }

        public async Task<GetApplicationResponse> Handle(GetApplicationRequest query, CancellationToken cancellationToken = default)
        {
            var application = await _applicationQueryRepository.Get(app => app.Id == query.ApplicationId && app.AccountId == query.AccountId);

            var legalEntity = await _legalEntityQueryRepository.Get(x => x.AccountLegalEntityId == application.AccountLegalEntityId);
            application.NewAgreementRequired = Incentive.IsNewAgreementRequired(application, legalEntity);

            var response = new GetApplicationResponse(application);

            return response;
        }
    }
}
