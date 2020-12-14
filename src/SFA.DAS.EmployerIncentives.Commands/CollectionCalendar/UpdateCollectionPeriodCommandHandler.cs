using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.CollectionPeriod;
using SFA.DAS.EmployerIncentives.Commands.Services;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.CollectionCalendar
{
    public class UpdateCollectionPeriodCommandHandler : ICommandHandler<UpdateCollectionPeriodCommand>
    {
        private readonly ICollectionCalendarService _collectionCalendarService;
        public UpdateCollectionPeriodCommandHandler(ICollectionCalendarService collectionCalendarService)
        {
            _collectionCalendarService = collectionCalendarService;
        }

        public async Task Handle(UpdateCollectionPeriodCommand command, CancellationToken cancellationToken = default)
        {
            var collectionCalendar = await _collectionCalendarService.Get();
            var collectionPeriod = new Domain.ValueObjects.CollectionPeriod(command.CollectionPeriodNumber, command.CollectionPeriodYear);

            if (command.Active) 
            {
                collectionCalendar.SetActive(collectionPeriod);

                await _collectionCalendarService.Save(collectionCalendar);
            }
        }
    }
}
