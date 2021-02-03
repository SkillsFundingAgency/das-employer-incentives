using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Data.Map;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Queries.Account.GetAccountsWithVrfStatus
{
    public class GetAccountsWithVrfCaseStatusQueryHandler : IQueryHandler<GetAccountsWithVrfCaseStatusRequest, GetAccountsWithVrfCaseStatusResponse>
    {
        private readonly IAccountDataRepository _accountDataRepository;

        public GetAccountsWithVrfCaseStatusQueryHandler(IAccountDataRepository accountDataRepository)
        {
            _accountDataRepository = accountDataRepository;
        }

        public async Task<GetAccountsWithVrfCaseStatusResponse> Handle(GetAccountsWithVrfCaseStatusRequest query, CancellationToken cancellationToken = default)
        {
            var accounts = await _accountDataRepository.GetByVrfCaseStatus(query.VrfStatus);
            
            var response = new GetAccountsWithVrfCaseStatusResponse { Accounts = accounts };

            return await Task.FromResult(response);
        }
    }
}
