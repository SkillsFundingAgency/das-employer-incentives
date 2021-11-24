using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Queries.Decorators
{
    public class QueryHandlerWithLoggingArgs<TQuery, TResult> : IQueryHandler<TQuery, TResult> where TQuery : IQuery
    {
        private readonly IQueryHandler<TQuery, TResult> _handler;
        private readonly ILogger<TQuery> _log;

        public QueryHandlerWithLoggingArgs(
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
                var queryLog = (query as IRequestLogWriterWithArgs).Log;

                try
                {
                    var processing = queryLog.OnProcessing.Invoke();

                    _log.LogDebug($"Start handle query '{typeof(TQuery)}' : {processing.Item1}", processing.Item2);

                    var result = await _handler.Handle(query, cancellationToken);

                    var responseLog = (result is IResponseLogWriterWithArgs) ? (result as IResponseLogWriterWithArgs).Log : new ResponseLogWithArgs();

                    var processed = responseLog.OnProcessed.Invoke();

                    _log.LogDebug($"End handle query '{typeof(TQuery)}'; Request : {processing.Item1}; Response :- {processed.Item1}", processing.Item2.Concat(processed.Item2).ToArray());

                    return result;
                }
                catch (Exception ex)
                {
                    var errored = queryLog.OnError.Invoke();

                    _log.LogError(ex, $"Error handling query '{typeof(TQuery)}' : {errored}", errored.Item2);

                    throw;
                }
            }
            else
            {
                return await _handler.Handle(query, cancellationToken);
            }
        }
    }
}
