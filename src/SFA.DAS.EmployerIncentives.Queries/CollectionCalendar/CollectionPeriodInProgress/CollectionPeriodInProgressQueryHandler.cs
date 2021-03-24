using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;

namespace SFA.DAS.EmployerIncentives.Queries.CollectionCalendar.CollectionPeriodInProgress
{
    public class CollectionPeriodInProgressQueryHandler : IQueryHandler<CollectionPeriodInProgressRequest, CollectionPeriodInProgressResponse>
    {
        private readonly ICollectionCalendarService _collectionCalendarService;

        public CollectionPeriodInProgressQueryHandler(ICollectionCalendarService collectionCalendarService)
        {
            _collectionCalendarService = collectionCalendarService;
        }

        public async Task<CollectionPeriodInProgressResponse> Handle(CollectionPeriodInProgressRequest query, CancellationToken cancellationToken = default)
        {
            var collectionCalendar = await _collectionCalendarService.Get();
            var activePeriod = collectionCalendar.GetActivePeriod();

            return new CollectionPeriodInProgressResponse(activePeriod.PeriodEndInProgress);
        }
    }
}
