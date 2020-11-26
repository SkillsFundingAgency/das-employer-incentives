using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.CollectionPeriod;
using SFA.DAS.EmployerIncentives.Commands.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.CollectionCalendar
{
    public class ActivateCollectionPeriodCommandHandler : ICommandHandler<ActivateCollectionPeriodCommand>
    {
        private readonly ICollectionCalendarService _collectionCalendarService;
        public ActivateCollectionPeriodCommandHandler(ICollectionCalendarService collectionCalendarService)
        {
            _collectionCalendarService = collectionCalendarService;
        }

        public async Task Handle(ActivateCollectionPeriodCommand command, CancellationToken cancellationToken = default)
        {
            var collectionCalendar = await _collectionCalendarService.Get();

            collectionCalendar.ActivatePeriod(command.CollectionPeriodYear, command.CollectionPeriodNumber);

            await _collectionCalendarService.Save(collectionCalendar);
        }
    }
}
