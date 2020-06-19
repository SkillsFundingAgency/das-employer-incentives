using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Domain.Entities;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Application.Persistence
{
    public class AccountDomainRepository : IDomainRepository<long, Account>
    {
        private readonly IAccountDataRepository _accountDataRepository;
        public AccountDomainRepository(IAccountDataRepository accountDataRepository)
        {
            _accountDataRepository = accountDataRepository;
        }

        public async Task<Account> Find(long id)
        {
            var model = await _accountDataRepository.Find(id);

            return model == null ? null : Account.Create(model);

        }

        public Task Save(Account aggregate)
        {
            if (aggregate.IsNew)
            {               
                return _accountDataRepository.Add(aggregate.GetModel());
            }

            return _accountDataRepository.Update(aggregate.GetModel());
        }
    }
  
}
