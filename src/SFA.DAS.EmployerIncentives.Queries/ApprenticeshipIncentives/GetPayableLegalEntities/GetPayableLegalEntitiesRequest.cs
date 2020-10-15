using SFA.DAS.EmployerIncentives.Abstractions.Queries;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetPayableLegalEntities
{
    public class GetPayableLegalEntitiesRequest : IQuery
    {
        public short CollectionPeriodYear { get; }
        public byte CollectionPeriodMonth { get; }

        public GetPayableLegalEntitiesRequest(short collectionPeriodYear, byte collectionPeriodMonth)
        {
            CollectionPeriodYear = collectionPeriodYear;
            CollectionPeriodMonth = collectionPeriodMonth;
        }
    }
}
