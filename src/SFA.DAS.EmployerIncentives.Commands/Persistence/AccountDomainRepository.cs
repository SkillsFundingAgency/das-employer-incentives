using SFA.DAS.EmployerIncentives.Abstractions.Events;
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
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        
        public AccountDomainRepository(
            IAccountDataRepository accountDataRepository,
            IDomainEventDispatcher domainEventDispatcher)
        {
            _accountDataRepository = accountDataRepository;
            _domainEventDispatcher = domainEventDispatcher;
        }

        public async Task<Account> Find(long id)
        {
            var model = await _accountDataRepository.Find(id);

            return model == null ? null : Account.Create(model);

        }

        public async Task Save(Account aggregate)
        {
            if (aggregate.IsNew)
            {
                await _accountDataRepository.Add(aggregate.GetModel());
            }
            else
            {
                await _accountDataRepository.Update(aggregate.GetModel());
            }

            foreach (dynamic domainEvent in aggregate.FlushEvents())
            {
                await _domainEventDispatcher.Send(domainEvent);
            }
        }

        public async Task<IEnumerable<Account>> GetByLegalEntityId(long legalEntityId)
        {
            var data = await _accountDataRepository.GetByLegalEntityId(legalEntityId);

            return data.Select(Account.Create);
        }

        public async Task<IEnumerable<Account>> GetByHashedLegalEntityId(string hashedLegalEntityId)
        {
            var data = await _accountDataRepository.GetByHashedLegalEntityId(hashedLegalEntityId);

            return data.Select(Account.Create);
        }
    }
}
