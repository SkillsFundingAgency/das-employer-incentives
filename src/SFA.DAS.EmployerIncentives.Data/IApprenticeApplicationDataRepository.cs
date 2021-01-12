using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data
{
    public interface IApprenticeApplicationDataRepository
    {
        Task<List<ApprenticeApplicationDto>> GetList(long accountId, long accountLegalEntityId);
    }
}
