using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Queries.Decorators
{
    public class QueryHandlerWithRetry<TQuery, TResult> : IQueryHandler<TQuery, TResult> where TQuery : IQuery
    {
        private readonly IQueryHandler<TQuery, TResult> _handler;
        private readonly Policies _policies;

        public QueryHandlerWithRetry(
            IQueryHandler<TQuery, TResult> handler,
            Policies policies)
        {
            _handler = handler;
            _policies = policies;
        }

        public Task<TResult> Handle(TQuery query, CancellationToken cancellationToken = default)
        {
            return _policies.QueryRetryPolicy.ExecuteAsync((cancellationToken) => _handler.Handle(query, cancellationToken), cancellationToken);
        }


    }
}