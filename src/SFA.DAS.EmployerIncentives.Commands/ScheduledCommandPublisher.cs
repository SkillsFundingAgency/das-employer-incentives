using System;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using ICommand = SFA.DAS.EmployerIncentives.Abstractions.Commands.ICommand;

namespace SFA.DAS.EmployerIncentives.Commands
{
    public class ScheduledCommandPublisher : IScheduledCommandPublisher
    {
        private readonly IMessageSession _messageSession;

        public ScheduledCommandPublisher(IMessageSession messageSession)
        {
            _messageSession = messageSession;
        }

        public async Task Send<T>(T command, TimeSpan delay, CancellationToken cancellationToken = default(CancellationToken)) where T : class, ICommand
        {
            var sendOptions = new SendOptions();
            sendOptions.DelayDeliveryWith(delay);
            await _messageSession.Send(command, sendOptions);
        }
    }
}
