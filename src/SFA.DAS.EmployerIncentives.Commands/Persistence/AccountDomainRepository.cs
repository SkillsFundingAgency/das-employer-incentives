using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Persistence
{
    public class AccountDomainRepository : IAccountDomainRepository
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

        public async Task<IEnumerable<Account>> GetByLegalEntityId(long legalEntityId)
        {
            var data = await _accountDataRepository.GetByLegalEntityId(legalEntityId);

            return data.Select(Account.Create);
        }
    }

}
