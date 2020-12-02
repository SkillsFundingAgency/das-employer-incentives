using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public interface IPayableLegalEntityQueryRepository
    {
        Task<List<PayableLegalEntityDto>> GetList(short collectionPeriodYear, byte collectionPeriodNumber);
    }
}
