using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.DataTransferObjects;

namespace SFA.DAS.EmployerIncentives.Data
{
    public interface IAccountDataRepository
    {
        Task Update(AccountModel account);
        Task Add(AccountModel account);
        Task<AccountModel> Find(long accountId);
        Task<IEnumerable<AccountModel>> GetByHashedLegalEntityId(string hashedLegalEntityId);
        Task<IEnumerable<DataTransferObjects.Account>> GetByVrfCaseStatus(string vrfCaseStatus);       
        Task<DateTime?> GetLatestVendorRegistrationCaseUpdateDateTime();
    }
}
