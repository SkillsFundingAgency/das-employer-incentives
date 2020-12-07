using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Queries.Account.GetAccountsWithVrfStatus
{
    public class GetAccountsWithVrfCaseStatusResponse
    {
        public IEnumerable<AccountDto> Accounts { get; set; }
    }
}
