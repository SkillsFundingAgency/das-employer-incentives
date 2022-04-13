using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries;

namespace SFA.DAS.EmployerIncentives.Data
{
    public interface IApprenticeApplicationDataRepository
    {
        Task<List<ApprenticeApplication>> GetList(long accountId, long accountLegalEntityId);
        Task<Guid?> GetFirstSubmittedApplicationId(long accountLegalEntityId);
    }
}
