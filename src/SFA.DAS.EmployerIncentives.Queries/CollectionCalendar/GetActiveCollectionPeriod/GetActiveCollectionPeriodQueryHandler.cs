using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;

namespace SFA.DAS.EmployerIncentives.Queries.CollectionCalendar.GetActiveCollectionPeriod
{
    public class GetActiveCollectionPeriodQueryHandler : IQueryHandler<GetActiveCollectionPeriodRequest, GetActiveCollectionPeriodResponse>
    {
        private readonly ICollectionCalendarService _collectionCalendarService;
        
        public GetActiveCollectionPeriodQueryHandler(ICollectionCalendarService collectionCalendarService)
        {
            _collectionCalendarService = collectionCalendarService;
        }

        public async Task<GetActiveCollectionPeriodResponse> Handle(GetActiveCollectionPeriodRequest query, CancellationToken cancellationToken = default)
        {
            var collectionCalendar = await _collectionCalendarService.Get();
            var activePeriod = collectionCalendar.GetActivePeriod();
            return new GetActiveCollectionPeriodResponse(new CollectionPeriod() 
            {
                CollectionPeriodNumber = activePeriod.CollectionPeriod.PeriodNumber,
                CollectionYear = activePeriod.CollectionPeriod.AcademicYear,
                IsInProgress = activePeriod.PeriodEndInProgress
            });
        }
    }
}
