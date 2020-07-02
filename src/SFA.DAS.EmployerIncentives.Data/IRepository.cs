//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Threading.Tasks;
//using SFA.DAS.EmployerIncentives.Domain.Interfaces;

//namespace SFA.DAS.EmployerIncentives.Data
//{
//    public interface IRepository<TEntity> : IQueryRepository<TEntity> where TEntity : IAggregateRoot, IDisposable
//    {
//        Task<TEntity> Get(Expression<Func<TEntity, bool>> predicate);
//        Task<IEnumerable<TEntity>> GetList(Expression<Func<TEntity, bool>> predicate);
//        IQueryable<TEntity> Queryable(Expression<Func<TEntity, bool>> predicate);
//        void Add(TEntity entity);
//        void AddRange(IEnumerable<TEntity> entities);
//        void Remove(TEntity entity);
//        void RemoveRange(IEnumerable<TEntity> entities);
//        void Update(TEntity entity);
//        Task<int> SaveChangesAsync();
//    }
//}
