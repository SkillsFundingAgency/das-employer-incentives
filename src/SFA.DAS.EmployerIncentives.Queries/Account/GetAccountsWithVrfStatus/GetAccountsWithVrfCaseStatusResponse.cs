using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.DataTransferObjects;

namespace SFA.DAS.EmployerIncentives.Queries.Account.GetAccountsWithVrfStatus
{
    public class GetAccountsWithVrfCaseStatusResponse
    {
        public IEnumerable<AccountDto> Accounts { get; set; }
    }
}
