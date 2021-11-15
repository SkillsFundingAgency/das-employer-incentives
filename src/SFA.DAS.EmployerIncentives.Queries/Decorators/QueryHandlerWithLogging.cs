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
            var domainLog = (query is ILogWriter) ? (query as ILogWriter).Log : new Log();

            try
            {
                if (domainLog.OnProcessing == null)
                {
                    _log.LogDebug($"Start handle '{typeof(TQuery)}' query");
                }
                else
                {
                    _log.LogDebug($"Start handle '{typeof(TQuery)}' query : {domainLog.OnProcessing.Invoke()}");
                }

                var result = await _handler.Handle(query, cancellationToken);

                if (domainLog.OnProcessed == null)
                {
                    _log.LogDebug($"End handle '{typeof(TQuery)}' query");
                }
                else
                {
                    _log.LogDebug($"End handle '{typeof(TQuery)}' query : {domainLog.OnProcessed.Invoke()}");
                }
                return result;
            }
            catch (Exception ex)
            {
                if (domainLog.OnError == null)
                {
                    _log.LogError(ex, $"Error handling '{typeof(TQuery)}' query");
                }
                else
                {
                    _log.LogError(ex, $"Error handling '{typeof(TQuery)}' query : {domainLog.OnError.Invoke()}");
                }

                throw;
            }
        }
    }
}
