using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.Withdrawals;
using SFA.DAS.EmployerIncentives.Data.IncentiveApplication;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Persistence
{
    public class IncentiveApplicationDomainRepository : IIncentiveApplicationDomainRepository
    {
        private readonly IIncentiveApplicationDataRepository _incentiveApplicationDataRepository;
        private readonly IIncentiveApplicationFactory _incentiveApplicationFactory;
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public IncentiveApplicationDomainRepository(
            IIncentiveApplicationDataRepository incentiveApplicationDataRepository,
            IIncentiveApplicationFactory incentiveApplicationFactory,
            IDomainEventDispatcher domainEventDispatcher)
        {
            _incentiveApplicationDataRepository = incentiveApplicationDataRepository;
            _incentiveApplicationFactory = incentiveApplicationFactory;
            _domainEventDispatcher = domainEventDispatcher;
        }

        public async Task<IncentiveApplication> Find(Guid id)
        {
            var application = await _incentiveApplicationDataRepository.Get(id);
            if (application != null)
            {
                return await Task.FromResult(_incentiveApplicationFactory.GetExisting(id, application));
            }

            return null;
        }

        public async Task<IEnumerable<IncentiveApplication>> Find(WithdrawalCommand withdrawalCommand)
        {
            var applications = await _incentiveApplicationDataRepository.FindApplicationsByAccountLegalEntityAndUln(withdrawalCommand.AccountLegalEntityId, withdrawalCommand.ULN);

            return (from app in applications
                    select _incentiveApplicationFactory.GetExisting(app.Id, app));
        }

        public async Task<IEnumerable<IncentiveApplication>> FindByAccountLegalEntity(long accountLegalEntityId)
        {
            var applications = await _incentiveApplicationDataRepository.FindApplicationsByAccountLegalEntity(accountLegalEntityId);

            return (from app in applications
                select _incentiveApplicationFactory.GetExisting(app.Id, app));
        }

        public async Task<List<IncentiveApplication>> FindIncentiveApplicationsWithoutEarningsCalculations()
        {
            var applications = await _incentiveApplicationDataRepository.FindApplicationsWithoutEarningsCalculated();
            
            return (from app in applications
                    select _incentiveApplicationFactory.GetExisting(app.Id, app)).ToList();
        }

        public async Task Save(IncentiveApplication aggregate)
        {
            if (aggregate.IsNew)
            {
                await _incentiveApplicationDataRepository.Add(aggregate.GetModel());
            }
            else
            {
                await _incentiveApplicationDataRepository.Update(aggregate.GetModel());
            }

            foreach (dynamic domainEvent in aggregate.FlushEvents())
            {
                await _domainEventDispatcher.Send(domainEvent);
            }
        }
    }  
}
