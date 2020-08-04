using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data
{
    public interface IQueryRepository<T>
    {
        Task<T> Get(Expression<Func<T, bool>> predicate);
        Task<List<T>> GetList(Expression<Func<T, bool>> predicate = null);
        //// IQueryable<TEntity> Queryable(Expression<Func<TEntity, bool>> predicate);
    }
}
