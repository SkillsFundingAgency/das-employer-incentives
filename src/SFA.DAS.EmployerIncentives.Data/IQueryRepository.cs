using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions;

namespace SFA.DAS.EmployerIncentives.Data
{
    public interface IQueryRepository<T>
    {
       //// Task<TEntity> Get(Expression<Func<TEntity, bool>> predicate);
        Task<List<LegalEntityDto>> GetList(Expression<Func<T, bool>> predicate = null);
        //// IQueryable<TEntity> Queryable(Expression<Func<TEntity, bool>> predicate);
    }
}
