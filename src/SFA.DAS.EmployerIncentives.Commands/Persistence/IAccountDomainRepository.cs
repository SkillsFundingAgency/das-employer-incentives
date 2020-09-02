using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.Accounts;

namespace SFA.DAS.EmployerIncentives.Commands.Persistence
{
    public interface IAccountDomainRepository : IDomainRepository<long, Account>
    {
    }
}
