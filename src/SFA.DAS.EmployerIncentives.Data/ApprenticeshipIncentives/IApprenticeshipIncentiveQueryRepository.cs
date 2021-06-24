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

        Task<Models.ApprenticeshipIncentive> Get(Expression<Func<Models.ApprenticeshipIncentive, bool>> predicate, bool includePayments = false);

        Task<List<ApprenticeshipIncentiveDto>> GetWithdrawable(long accountId, long accountLegalEntityId);
    }
}
