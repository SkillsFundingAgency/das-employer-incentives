using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.EarningsResilienceCheck
{
    public interface IEarningsResilienceCheckRepository
    {
        Task<IEnumerable<Guid>> GetApplicationsWithoutEarningsCalculations();
        //Task<IncentiveApplicationModel> GetApplicationDetail(Guid applicationId);
    }
}
