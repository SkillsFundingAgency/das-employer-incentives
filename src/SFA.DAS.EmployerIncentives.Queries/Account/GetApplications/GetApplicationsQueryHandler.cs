using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Queries.Account.GetApplications
{
    public class GetApplicationsQueryHandler : IQueryHandler<GetApplicationsRequest, GetApplicationsResponse>
    {
        private readonly IApprenticeApplicationDataRepository _applicationRepository;
        private readonly IAccountDataRepository _accountRepository;

        public GetApplicationsQueryHandler(IApprenticeApplicationDataRepository applicationRepository, IAccountDataRepository accountRepository)
        {
            _applicationRepository = applicationRepository;
            _accountRepository = accountRepository;
        }

        public async Task<GetApplicationsResponse> Handle(GetApplicationsRequest query, CancellationToken cancellationToken = default)
        {
            var applications = await _applicationRepository.GetList(query.AccountId, query.AccountLegalEntityId);

            var account = await _accountRepository.Find(query.AccountId);
            var accountLegalEntity = account.LegalEntityModels.FirstOrDefault(x => x.AccountLegalEntityId == query.AccountLegalEntityId);
            
            var response = new GetApplicationsResponse
            {
                ApprenticeApplications = applications,
                BankDetailsStatus = accountLegalEntity.BankDetailsStatus,
                FirstSubmittedApplicationId = await _applicationRepository.GetFirstSubmittedApplicationId(query.AccountLegalEntityId)
            };

            return response;
        }
    }
}
