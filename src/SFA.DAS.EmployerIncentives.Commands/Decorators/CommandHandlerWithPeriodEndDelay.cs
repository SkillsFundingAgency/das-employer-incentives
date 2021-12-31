using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Decorators
{
    public class CommandHandlerWithPeriodEndDelay<T> : ICommandHandler<T> where T : ICommand
    {
        private readonly ICommandHandler<T> _handler;
        private readonly IScheduledCommandPublisher _scheduledCommandPublisher;
        private readonly ICollectionCalendarService _collectionCalendarService;

        public CommandHandlerWithPeriodEndDelay(
            ICommandHandler<T> handler,
            IScheduledCommandPublisher scheduledCommandPublisher,
            ICollectionCalendarService collectionCalendarService)
        {
            _handler = handler;
            _scheduledCommandPublisher = scheduledCommandPublisher;
            _collectionCalendarService = collectionCalendarService;
        }

        public async Task Handle(T command, CancellationToken cancellationToken = default)
        {
            if (command is IPeriodEndIncompatible delayable)
            {
                if (await ActivePeriodInProgress())
                {
                    if (delayable.CancelCommand)
                    {
                        return;
                    }
                    await _scheduledCommandPublisher.Send(command, delayable.CommandDelay, cancellationToken);
                }
                else
                {
                    await _handler.Handle(command, cancellationToken);
                }
            }
            else
            {
                await _handler.Handle(command, cancellationToken);
            }
        }

        private async Task<bool> ActivePeriodInProgress()
        {
            var collectionCalendar = await _collectionCalendarService.Get();
            var activePeriod = collectionCalendar.GetActivePeriod();
            return activePeriod.PeriodEndInProgress;
        }
    }
}
