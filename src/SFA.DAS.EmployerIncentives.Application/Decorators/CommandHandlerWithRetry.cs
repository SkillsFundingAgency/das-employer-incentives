using SFA.DAS.EmployerIncentives.Application.Commands;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Application.Decorators
{
    public class CommandHandlerWithRetry<T> : ICommandHandler<T> where T : ICommand
    {
        private readonly ICommandHandler<T> _handler;
        private readonly Policies _policies;

        public CommandHandlerWithRetry(
            ICommandHandler<T> handler,
            Policies policies)
        {
            _handler = handler;
            _policies = policies;
        }

        public Task Handle(T command)
        {
            return _policies.LockRetryPolicy.ExecuteAsync(() => _handler.Handle(command));
        }
    }
}
