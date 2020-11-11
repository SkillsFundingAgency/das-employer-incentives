using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;

namespace SFA.DAS.EmployerIncentives.Commands.Persistence
{
    public interface IApprenticeshipIncentiveDomainRepository : IDomainRepository<Guid, Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>
    {
        Task<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive> FindByApprenticeshipId(Guid incentiveApplicationApprenticeshipId);

        Task<List<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>> FindIncentivesWithoutPendingPayments();
    }
}
