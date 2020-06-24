using SFA.DAS.EmployerIncentives.Domain.Accounts;

namespace SFA.DAS.EmployerIncentives.Application.Persistence
{
    public interface IAccountDomainRepository : IDomainRepository<long, Account>
    {
    }
}
