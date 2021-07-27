using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public interface IApprenticeshipIncentiveQueryRepository : IQueryRepository<Models.ApprenticeshipIncentive>
    {
        Task<List<ApprenticeshipIncentiveDto>> GetList();        
        Task<List<ApprenticeshipIncentiveDto>> GetDtoList(Expression<Func<Models.ApprenticeshipIncentive, bool>> predicate = null);

        Task<Models.ApprenticeshipIncentive> Get(Expression<Func<Models.ApprenticeshipIncentive, bool>> predicate, bool includePayments = false);
    }
}
