using SFA.DAS.EmployerIncentives.Abstractions.Queries;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetPayableLegalEntities
{
    public class GetPayableLegalEntitiesRequest : IQuery
    {
        public int CollectionPeriodYear { get; }
        public int CollectionPeriodMonth { get; }

        public GetPayableLegalEntitiesRequest(int collectionPeriodYear, int collectionPeriodMonth)
        {
            CollectionPeriodYear = collectionPeriodYear;
            CollectionPeriodMonth = collectionPeriodMonth;
        }
    }
}
