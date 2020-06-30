using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace SFA.DAS.EmployerIncentives.Queries
{
    public class QueryDispatcher : IQueryDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public QueryDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<TResult> Send<TQuery, TResult>(TQuery query) where TQuery : IQuery<TResult>
        {
            var service = this._serviceProvider.GetService<IQueryHandler<TQuery, TResult>>();
            
            // TODO: add null-check

            return await service.Handle(query);
        }
    }
}