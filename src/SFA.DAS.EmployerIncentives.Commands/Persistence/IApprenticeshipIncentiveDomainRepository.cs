using System;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;

namespace SFA.DAS.EmployerIncentives.Commands.Persistence
{
    public interface IApprenticeshipIncentiveDomainRepository : IDomainRepository<Guid, Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>
    {
    }
}
