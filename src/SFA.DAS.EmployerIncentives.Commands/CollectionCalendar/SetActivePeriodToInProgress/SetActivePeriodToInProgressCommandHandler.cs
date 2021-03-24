using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;

namespace SFA.DAS.EmployerIncentives.Commands.CollectionCalendar.SetActivePeriodToInProgress
{
    public class SetActivePeriodToInProgressCommandHandler : ICommandHandler<SetActivePeriodToInProgressCommand>
    {
        private readonly ICollectionCalendarService _collectionCalendarService;

        public SetActivePeriodToInProgressCommandHandler(ICollectionCalendarService collectionCalendarService)
        {
            _collectionCalendarService = collectionCalendarService;
        }

        public async Task Handle(SetActivePeriodToInProgressCommand command, CancellationToken cancellationToken = default)
        {
            var collectionCalendar = await _collectionCalendarService.Get();
            collectionCalendar.SetActivePeriodToInProgress();
            await _collectionCalendarService.Save(collectionCalendar);
        }
    }
}
