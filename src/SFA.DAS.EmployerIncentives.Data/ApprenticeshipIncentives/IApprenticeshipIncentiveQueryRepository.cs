using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public interface IApprenticeshipIncentiveQueryRepository : IQueryRepository<Models.ApprenticeshipIncentive>
    {
        Task<List<ApprenticeshipIncentive>> GetList();        
        Task<List<ApprenticeshipIncentive>> GetDtoList(Expression<Func<Models.ApprenticeshipIncentive, bool>> predicate = null);

        Task<Models.ApprenticeshipIncentive> Get(Expression<Func<Models.ApprenticeshipIncentive, bool>> predicate, bool includePayments = false);
    }
}
