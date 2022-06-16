using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.IncentiveApplication
{
    public interface IIncentiveApplicationStatusAuditDataRepository
    {
        Task Add(IncentiveApplicationAudit incentiveApplicationAudit);
        List<IncentiveApplicationStatusAudit> GetByApplicationApprenticeshipId(Guid incentiveApplicationApprenticeshipId);
    }
}
