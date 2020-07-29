using System;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;

namespace SFA.DAS.EmployerIncentives.Commands.Persistence
{
    public interface IIncentiveApplicationDomainRepository : IDomainRepository<Guid, IncentiveApplication>
    {
    }
}
