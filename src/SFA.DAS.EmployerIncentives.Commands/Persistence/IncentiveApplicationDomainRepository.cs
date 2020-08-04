using System;
using SFA.DAS.EmployerIncentives.Data;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;

namespace SFA.DAS.EmployerIncentives.Commands.Persistence
{
    public class IncentiveApplicationDomainRepository : IIncentiveApplicationDomainRepository
    {
        private readonly IIncentiveApplicationDataRepository _incentiveApplicationDataRepository;

        public IncentiveApplicationDomainRepository(IIncentiveApplicationDataRepository incentiveApplicationDataRepository)
        {
            _incentiveApplicationDataRepository = incentiveApplicationDataRepository;
        }

        public async Task<IncentiveApplication> Find(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task Save(IncentiveApplication aggregate)
        {
            if (aggregate.IsNew)
            {               
                return _incentiveApplicationDataRepository.Add(aggregate.GetModel());
            }

            throw new NotImplementedException();
        }
    }
  
}
