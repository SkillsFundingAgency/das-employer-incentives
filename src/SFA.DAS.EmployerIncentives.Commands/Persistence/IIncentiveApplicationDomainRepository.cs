using System;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplication;

namespace SFA.DAS.EmployerIncentives.Commands.Persistence
{
    public interface IIncentiveApplicationDomainRepository : IDomainRepository<Guid, IncentiveApplication>
    {
    }
}
