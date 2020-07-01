using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data
{
    public interface IReadonlyRepository<TEntity> : IDisposable
    {
       //// TODO: Task<TEntity> Get(Expression<Func<TEntity, bool>> predicate);
        Task<IEnumerable<TEntity>> GetList(Expression<Func<TEntity, bool>> predicate);
        //// TODO: IQueryable<TEntity> Queryable(Expression<Func<TEntity, bool>> predicate);
    }

    //public class ReadonlyRepository<TEntity> : IReadonlyRepository<TEntity>
    //{
    //    private IReadonlyRepository<TEntity> _readonlyRepositoryImplementation;
    //    public Task<IEnumerable<TEntity>> GetList(Expression<Func<TEntity, bool>> predicate)
    //    {
    //        return _readonlyRepositoryImplementation.GetList(predicate);
    //    }

    //    public void Dispose()
    //    {
    //        _readonlyRepositoryImplementation.Dispose();
    //    }
    //}
}
