using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.EarningsResilienceCheck
{
    public interface IEarningsResilienceCheckRepository
    {
        Task<IEnumerable<EarningsResilienceCheckDto>> GetApplicationsWithoutEarningsCalculations();
    }
}
