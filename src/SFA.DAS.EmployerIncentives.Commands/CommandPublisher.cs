using NServiceBus;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands
{
    public class CommandPublisher<TCommand> : ICommandPublisher<TCommand>
    {
        private readonly IMessageSession _messageSession;

        public CommandPublisher(IMessageSession messageSession)
        {
            _messageSession = messageSession;
        }

        public Task Publish(TCommand command, CancellationToken cancellationToken = default)
        {
            return _messageSession.Send(command);
        }
    }
}
