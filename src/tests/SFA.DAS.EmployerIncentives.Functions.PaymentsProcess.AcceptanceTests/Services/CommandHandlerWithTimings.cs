using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Services
{
    public class CommandHandlerWithTimings<T> : ICommandHandler<T> where T : ICommand
    {
        private readonly ICommandHandler<T> _handler;
        private readonly ILogger<T> _log;

        public CommandHandlerWithTimings(
            ICommandHandler<T> handler,         
            ILogger<T> log)
        {
            _handler = handler;
            _log = log;
        }

        public async Task Handle(T command, CancellationToken cancellationToken = default)
        {
            var timer = new Stopwatch();

            timer.Start();

            await _handler.Handle(command, cancellationToken);

            timer.Stop();

            _log.LogInformation($"'{typeof(T)}' command took : {timer.ElapsedMilliseconds}ms");
        }
    }
}
