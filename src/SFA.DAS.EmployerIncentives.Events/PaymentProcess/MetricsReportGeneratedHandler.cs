using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.Types.Notification.Messages;
using SFA.DAS.EmployerIncentives.Commands.Types.PaymentProcess;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.PaymentProcess
{
    public class MetricsReportGeneratedHandler : IDomainEventHandler<MetricsReportGenerated>
    {
        private readonly ICommandPublisher _commandPublisher;

        public MetricsReportGeneratedHandler(ICommandPublisher commandPublisher)
        {
            _commandPublisher = commandPublisher;
        }

        public Task Handle(MetricsReportGenerated @event, CancellationToken cancellationToken = default)
        {
            var message = new MetricsReportGeneratedMessage(@event.CollectionPeriod).Message;

            var command = new SlackNotificationCommand(message);

            return _commandPublisher.Publish(command);
        }
    }
}
