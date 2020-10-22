using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Abstractions.Domain
{
    public abstract class Specification<T>
    {
        public abstract string Name { get; }
        public abstract Expression<Func<T, bool>> ToExpression();

        protected dynamic Data { get; set; }

        public abstract Task<dynamic> Fetch(T entity);

        public async Task<bool> IsSatisfiedBy(T entity)
        {
            Data = await Fetch(entity);
            Func<T, bool> predicate = ToExpression().Compile();
            return predicate(entity);
        }
    }
}
