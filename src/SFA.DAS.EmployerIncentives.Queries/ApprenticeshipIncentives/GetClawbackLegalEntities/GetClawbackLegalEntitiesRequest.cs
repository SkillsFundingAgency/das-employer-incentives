using SFA.DAS.EmployerIncentives.Abstractions.Queries;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetClawbackLegalEntities
{
    public class GetClawbackLegalEntitiesRequest : IQuery
    {
        public bool IsSent { get; }
        public short CollectionPeriodYear { get; }
        public byte CollectionPeriodMonth { get; }

        public GetClawbackLegalEntitiesRequest(short collectionPeriodYear, byte collectionPeriodMonth, bool isSent = false)
        {
            CollectionPeriodYear = collectionPeriodYear;
            CollectionPeriodMonth = collectionPeriodMonth;
            IsSent = isSent;
        }
    }
}
