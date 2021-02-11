using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Commands.Withdrawals;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;

namespace SFA.DAS.EmployerIncentives.Commands.Persistence
{
    public interface IIncentiveApplicationDomainRepository : IDomainRepository<Guid, IncentiveApplication>
    {
        Task<List<IncentiveApplication>> FindIncentiveApplicationsWithoutEarningsCalculations();
        Task<IEnumerable<IncentiveApplication>> Find(WithdrawalCommand withdrawalCommand);
    }
}
