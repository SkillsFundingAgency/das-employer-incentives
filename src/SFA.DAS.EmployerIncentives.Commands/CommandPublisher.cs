using NServiceBus;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System.Threading;
using System.Threading.Tasks;
using ICommand = SFA.DAS.EmployerIncentives.Abstractions.Commands.ICommand;

namespace SFA.DAS.EmployerIncentives.Commands
{
    public class CommandPublisher : ICommandPublisher
    {
        private readonly IMessageSession _messageSession;

        public CommandPublisher(IMessageSession messageSession)
        {
            _messageSession = messageSession;
        }

        public Task Publish<T>(T command, CancellationToken cancellationToken = default) where T : ICommand
        {
            return _messageSession.Send(command);
        }

        public Task Publish(object command, CancellationToken cancellationToken = default)
        {
            return _messageSession.Send(command);
        }
    }
}
