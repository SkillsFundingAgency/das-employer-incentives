using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.NServiceBus.Services;
using ICommand = SFA.DAS.EmployerIncentives.Abstractions.Commands.ICommand;

namespace SFA.DAS.EmployerIncentives.Commands
{
    public class CommandPublisher : ICommandPublisher
    {
        private readonly IEventPublisher _eventPublisher;

        public CommandPublisher(IEventPublisher eventPublisher)
        {
            _eventPublisher = eventPublisher;
        }

        public Task Publish<T>(T command, CancellationToken cancellationToken = default) where T : class, ICommand
        {
            return _eventPublisher.Publish(command);
        }

        public Task Publish(object command, CancellationToken cancellationToken = default)
        {
            return _eventPublisher.Publish(command);
        }
    }
}
