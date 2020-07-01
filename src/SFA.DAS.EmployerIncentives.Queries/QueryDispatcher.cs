using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmployerIncentives.Queries.Exceptions;

namespace SFA.DAS.EmployerIncentives.Queries
{
    public class QueryDispatcher : IQueryDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public QueryDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task<TResult> Send<TQuery, TResult>(TQuery query) where TQuery : IQuery
        {
            var service = _serviceProvider.GetService<IQueryHandler<TQuery, TResult>>();

            if (service == null)
            {
                throw new QueryDispatcherException($"Unable to dispatch query '{query.GetType().Name}'. No matching handler found.");
            }

            return service.Handle(query);
        }
    }
}