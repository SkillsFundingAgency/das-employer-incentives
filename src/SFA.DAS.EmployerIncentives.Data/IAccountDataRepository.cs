using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data
{
    public interface IAccountDataRepository
    {
        Task Update(AccountModel account);
        Task Add(AccountModel account);
        Task<AccountModel> Find(long accountId);
        Task<IEnumerable<AccountModel>> GetByHashedLegalEntityId(string hashedLegalEntityId);
        Task<IEnumerable<AccountDto>> GetByVrfCaseStatus(string vrfCaseStatus);
    }
}
