using System;
using SFA.DAS.EmployerIncentives.Data;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Domain.Factories;

namespace SFA.DAS.EmployerIncentives.Commands.Persistence
{
    public class IncentiveApplicationDomainRepository : IIncentiveApplicationDomainRepository
    {
        private readonly IIncentiveApplicationDataRepository _incentiveApplicationDataRepository;
        private readonly IIncentiveApplicationFactory _incentiveApplicationFactory;

        public IncentiveApplicationDomainRepository(IIncentiveApplicationDataRepository incentiveApplicationDataRepository,
                                                    IIncentiveApplicationFactory incentiveApplicationFactory)
        {
            _incentiveApplicationDataRepository = incentiveApplicationDataRepository;
            _incentiveApplicationFactory = incentiveApplicationFactory;
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

        public Task Save(IncentiveApplication aggregate)
        {
            if (aggregate.IsNew)
            {               
                return _incentiveApplicationDataRepository.Add(aggregate.GetModel());
            }

            return _incentiveApplicationDataRepository.Update(aggregate.GetModel());
        }
    }
  
}
