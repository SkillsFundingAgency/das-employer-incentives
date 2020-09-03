using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Queries.Decorators
{
    public class QueryHandlerWithLogging<TQuery, TResult> : IQueryHandler<TQuery, TResult> where TQuery : IQuery
    {
        private readonly IQueryHandler<TQuery, TResult> _handler;
        private readonly ILogger<TQuery> _log;

        public QueryHandlerWithLogging(
            IQueryHandler<TQuery, TResult> handler,
            ILogger<TQuery> log)
        {
            _handler = handler;
            _log = log;
        }

        public async Task<TResult> Handle(TQuery query, CancellationToken cancellationToken = default)
        {
            try
            {
                _log.LogInformation($"Start handle '{typeof(TQuery)}' query");
                if (query is ILogWriter<TQuery> writer)
                {
                    writer.Write(_log);
                }
                var result = await _handler.Handle(query, cancellationToken);
                _log.LogInformation($"End handle '{typeof(TQuery)}' query");
                return result;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Error handling '{typeof(TQuery)}' query");
                throw;
            }
        }
    }
}
