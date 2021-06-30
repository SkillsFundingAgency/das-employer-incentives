using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Queries.CollectionCalendar.GetActiveCollectionPeriod
{
    public class GetActiveCollectionPeriodResponse
    {
        public CollectionPeriodDto CollectionPeriod { get; }

        public GetActiveCollectionPeriodResponse(CollectionPeriodDto collectionPeriod)
        {
            CollectionPeriod = collectionPeriod;
        }
    }
}
