using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetActiveCollectionPeriod
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
