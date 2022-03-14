using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Decorators
{
    [ExcludeFromCodeCoverage]
    public class CommandHandlerWithDistributedLockInitialiser<T> : ICommandHandler<T> where T : ICommand
    {
        private readonly ICommandHandler<T> _handler;
        private readonly IDistributedLockProvider _lockProvider;

        public CommandHandlerWithDistributedLockInitialiser(
            ICommandHandler<T> handler,
            IDistributedLockProvider lockProvider)
        {
            _handler = handler;
            _lockProvider = lockProvider;
        }

        public async Task Handle(T command, CancellationToken cancellationToken = default)
        {
            if (command is ILockIdentifier)
            {
                try
                {
                    await _lockProvider.Start();
                    await _handler.Handle(command, cancellationToken);
                }
                finally
                {
                    await _lockProvider.Stop();
                }
            }
            else
            {
                await _handler.Handle(command, cancellationToken);
            }
        }
    }
}
