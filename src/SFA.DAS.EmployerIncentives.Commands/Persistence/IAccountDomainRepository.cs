using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Persistence
{
    public interface IAccountDomainRepository : IDomainRepository<long, Account>
    {
        Task<IEnumerable<Account>> GetByLegalEntityId(long legalEntityId);
        Task<IEnumerable<Account>> GetByHashedLegalEntityId(string hashedLegalEntityId);
    }
}
