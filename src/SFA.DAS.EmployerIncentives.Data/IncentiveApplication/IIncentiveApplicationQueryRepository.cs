using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries;

namespace SFA.DAS.EmployerIncentives.Data.IncentiveApplication
{
    public interface IIncentiveApplicationQueryRepository
    {
        Task<DataTransferObjects.Queries.IncentiveApplication> Get(Expression<Func<DataTransferObjects.Queries.IncentiveApplication, bool>> predicate);
        Task<List<DataTransferObjects.Queries.IncentiveApplication>> GetList(Expression<Func<DataTransferObjects.Queries.IncentiveApplication, bool>> predicate = null);
    }
}
