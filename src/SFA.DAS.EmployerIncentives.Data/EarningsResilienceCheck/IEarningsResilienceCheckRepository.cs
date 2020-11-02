using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.EarningsResilienceCheck
{
    public interface IEarningsResilienceCheckRepository
    {
        Task<IEnumerable<Guid>> GetApplicationsWithoutEarningsCalculations();
        Task<IncentiveApplicationModel> GetApplicationDetail(Guid applicationId);
    }
}
