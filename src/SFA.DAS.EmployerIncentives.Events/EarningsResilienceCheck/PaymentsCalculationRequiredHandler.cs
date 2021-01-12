using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.EarningsResilienceCheck.Events;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.EarningsResilienceCheck
{
    public class PaymentsCalculationRequiredHandler : IDomainEventHandler<PaymentsCalculationRequired>
    {
        private readonly ICommandPublisher _commandPublisher;

        public PaymentsCalculationRequiredHandler(ICommandPublisher commandPublisher)
        {
            _commandPublisher = commandPublisher;
        }

        public async Task Handle(PaymentsCalculationRequired @event, CancellationToken cancellationToken = default)
        {
            var command = new CalculateEarningsCommand(@event.Model.Id);
            
            await _commandPublisher.Publish(command);            
        }
    }
}
