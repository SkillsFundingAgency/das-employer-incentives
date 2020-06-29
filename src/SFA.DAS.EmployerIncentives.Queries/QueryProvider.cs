using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace SFA.DAS.EmployerIncentives.Queries
{
    public class QueryProvider : IQueryProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public QueryProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }


        public Task<TResponse> Execute<TResponse, TQuery>(TQuery query)
        {
            var queryHandler = _serviceProvider.GetService<IQueryHandler<TQuery>>();


            return queryHandler.Execute<TResponse>(query);
        }
    }
}