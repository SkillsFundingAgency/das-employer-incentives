using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.CollectionPeriod;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;

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
            var academicPeriod = new Domain.ValueObjects.AcademicPeriod(command.PeriodNumber, command.AcademicYear);

            if (command.Active) 
            {
                collectionCalendar.SetActive(academicPeriod);

                await _collectionCalendarService.Save(collectionCalendar);
            }
        }
    }
}
