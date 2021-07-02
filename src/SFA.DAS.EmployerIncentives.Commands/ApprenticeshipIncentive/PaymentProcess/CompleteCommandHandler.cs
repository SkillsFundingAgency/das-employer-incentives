using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.PaymentProcess
{
    public class CompleteCommandHandler : ICommandHandler<CompleteCommand>
    {
        private readonly ICollectionCalendarService _collectionCalendarService;

        public CompleteCommandHandler(
            ICollectionCalendarService collectionCalendarService)
        {
            _collectionCalendarService = collectionCalendarService;
        }

        public async Task Handle(CompleteCommand command, CancellationToken cancellationToken = default)
        {
            var collectionCalendar = await _collectionCalendarService.Get();
            var currentPeriod = collectionCalendar.GetPeriod(command.CollectionPeriod.AcademicYear, command.CollectionPeriod.PeriodNumber);

            currentPeriod.SetMonthEndProcessingCompletedDate(command.CompletionDateTime);
            collectionCalendar.SetActive(collectionCalendar.GetNextPeriod(currentPeriod));

            await _collectionCalendarService.Save(collectionCalendar);
        }
    }
}
