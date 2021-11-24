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
            if (query is IRequestLogWriterWithArgs)
            {
                return await _handler.Handle(query, cancellationToken);
            }
            
            var queryLog = (query is IRequestLogWriter) ? (query as IRequestLogWriter).Log : new RequestLog();

            try
            {
                _log.LogDebug($"Start handle query '{typeof(TQuery)}' : {queryLog.OnProcessing.Invoke()}");

                var result = await _handler.Handle(query, cancellationToken);

                var responseLog = (result is IResponseLogWriter) ? (result as IResponseLogWriter).Log : new ResponseLog();
                _log.LogDebug($"End handle query '{typeof(TQuery)}' : {responseLog.OnProcessed.Invoke()}");

                return result;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Error handling query '{typeof(TQuery)}' : {queryLog.OnError.Invoke()}");

                throw;
            }
        }
    }
}
